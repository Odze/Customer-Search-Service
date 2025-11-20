using System.Collections.Concurrent;
using api.Analyzers;
using api.Entity;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Directory = Lucene.Net.Store.Directory;

namespace api.Index
{
    public sealed class CustomerLuceneIndex : IDisposable
    {
        private readonly LuceneVersion _version = LuceneVersion.LUCENE_48;

        private readonly Directory _directory;
        private readonly Analyzer _indexAnalyzer;
        private readonly Analyzer _queryAnalyzer;
        private readonly IndexWriter _writer;

        private IndexSearcher _searcher;
        private DirectoryReader  _reader;

        private readonly ConcurrentQueue<Action<IndexWriter>> _ops = new();
        private readonly CancellationTokenSource _cts = new();

        private const string ID_KEY = "id";
        private const string FIRST_NAME_KEY = "firstName";
        private const string LAST_NAME_KEY = "lastName";
        private const string EMAIL_KEY = "email";
        
        private const int PROCESS_OPERATIONS_LOOP_DELAY = 1000;
        private const int REFRESH_SEARCHER_LOOP_DELAY = 1000;

        private readonly string[] _searchFields =
        {
            FIRST_NAME_KEY,
            LAST_NAME_KEY,
            EMAIL_KEY
        };

        public CustomerLuceneIndex()
        {
            _directory = new RAMDirectory();

            _indexAnalyzer = new EdgeNGramAnalyzer();
            _queryAnalyzer = new StandardAnalyzer(_version);

            IndexWriterConfig config = new(_version, _indexAnalyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            _writer = new IndexWriter(_directory, config);
            _reader = DirectoryReader.Open(_writer, applyAllDeletes: true);
            _searcher = new IndexSearcher(_reader);
            
            Task.Run(ProcessOperationsLoop);
            Task.Run(RefreshSearcherLoop);
        }
        
        public void Index(CustomerEntity customer)
        {
            _ops.Enqueue(writer =>
            {
                Document doc = BuildDocument(customer);
                Term idTerm = new(ID_KEY, customer.Id.ToString());
                writer.UpdateDocument(idTerm, doc);
            });
        }
        
        public void Delete(int id)
        {
            _ops.Enqueue(writer =>
            {
                Term idTerm = new(ID_KEY, id.ToString());
                writer.DeleteDocuments(idTerm);
            });
        }

        private async Task ProcessOperationsLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                if (_ops.TryDequeue(out Action<IndexWriter>? op))
                {
                    op(_writer);
                }
                else
                {
                    await Task.Delay(PROCESS_OPERATIONS_LOOP_DELAY);
                }
            }
        }
        
        private async Task RefreshSearcherLoop()
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(REFRESH_SEARCHER_LOOP_DELAY);

                DirectoryReader? newReader = DirectoryReader.OpenIfChanged(_reader, _writer, applyAllDeletes: true);

                if (newReader == null) 
                    continue;
                
                DirectoryReader oldReader = _reader;

                _reader = newReader;
                _searcher = new IndexSearcher(_reader);

                oldReader.Dispose();
            }
        }
        
        public IReadOnlyList<int> Search(string query, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<int>();

            query = query.Trim().ToLowerInvariant();

            bool isEmail = query.Contains("@");

            Query luceneQuery =
                isEmail 
                ? new TermQuery(new Term(EMAIL_KEY, query)) 
                : BuildNamePrefixQuery(query);

            TopDocs hits = _searcher.Search(luceneQuery, maxResults);
            List<int> ids = new(hits.ScoreDocs.Length);

            foreach (ScoreDoc? hit in hits.ScoreDocs)
            {
                Document doc = _searcher.Doc(hit.Doc);
                if (int.TryParse(doc.Get(ID_KEY), out int id))
                    ids.Add(id);
            }

            return ids;
        }

        private Query BuildNamePrefixQuery(string query)
        {
            string[] terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string> wildcardTerms = terms.Select(t => QueryParserBase.Escape(t) + "*");
            string joined = string.Join(' ', wildcardTerms);

            MultiFieldQueryParser parser = new(_version, _searchFields, _queryAnalyzer)
            {
                DefaultOperator = Operator.AND
            };

            return parser.Parse(joined);
        }
        
        private static Document BuildDocument(CustomerEntity customer)
        {
            Document doc = new()
            {
                new StringField(ID_KEY, customer.Id.ToString(), Field.Store.YES),
                new TextField(FIRST_NAME_KEY, customer.FirstName ?? "", Field.Store.NO),
                new TextField(LAST_NAME_KEY, customer.LastName ?? "", Field.Store.NO)
            };

            foreach (string email in customer.Email)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    doc.Add(new StringField(EMAIL_KEY, email.ToLowerInvariant(), Field.Store.NO));
            }

            return doc;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _reader?.Dispose();
            _writer?.Dispose();
            _indexAnalyzer?.Dispose();
            _queryAnalyzer?.Dispose();
            _directory?.Dispose();
        }
    }
}
