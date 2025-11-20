using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace api.Analyzers
{
    public sealed class EdgeNGramAnalyzer : Analyzer
    {
        private readonly LuceneVersion _version = LuceneVersion.LUCENE_48;

        protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
        {
            StandardTokenizer source = new(_version, reader);

            TokenStream stream = new StandardFilter(_version, source);
            stream = new LowerCaseFilter(_version, stream);

            stream = new EdgeNGramTokenFilter(
                version: _version,
                input: stream,
                minGram: 2,
                maxGram: 30
            );

            return new TokenStreamComponents(source, stream);
        }
    }
}