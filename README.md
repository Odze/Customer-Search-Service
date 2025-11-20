# Customer Search Service

## 📥 Clone the Repository

Clone this repo and open it in **Rider** or **Visual Studio Code**:
```
    git clone <your-repo-url>
    cd <your-repo>
```
## ▶️ Run the Projects

### Backend (API)
- Open the **`api/`** project in Rider or Visual Studio.
- Set it as the startup project.
- Run (F5).
- The API will start on:
    - `http://localhost:6000`
    - or port is shown in the console.

## Database Service Simulator

This project uses a simulated customer database for development and testing.

A full simulator is provided here:

👉 https://gitlab.com/srudolfs/customer-database-simulator

## 🗂 Backend Architecture Diagram

<p align="center">
  <img src="./docs/diagram.svg" width="750" />
</p>

## 📁 Backend Project File Overview

### **Analyzers/**
- **EdgeNGramAnalyzer.cs**  
  Custom Lucene.NET analyzer that tokenizes text into edge n-grams to support fast prefix and partial-match search.

### **Controllers/**
- **SearchController.cs**  
  HTTP API endpoint that exposes the `/search` route and returns partial-match customer results.

### **Entity/**
- **CustomerEntity.cs**  
  Internal in-memory representation of a customer used by the repository and search pipeline.

### **Enums/**
- **RepoUpdateType.cs**  
  Enum describing update operations from external data sources (`Upsert` or `Delete`).

### **Extensions/**
- **CustomerExtensions.cs**  
  Utility extensions for converting between payload and entity.

### **Index/**
- **CustomerLuceneIndex.cs**  
  Lucene.NET RAM index responsible for indexing customers and performing fast queries.

### **Listeners/**
- **SocketEventListener.cs**  
  ZeroMQ subscriber service that listens for `CREATE`, `UPDATE`, and `DELETE` customer events and updates the repository + index.

### **Managers/**
- **CustomerEventManager.cs**  
  Handles processing of high-level customer update events and applies appropriate repository/index operations.
- **CustomerManager.cs**  
  Performs a full initial customer sync on startup, stores all customers in memory, and automatically updates the Lucene index by subscribing to repository events (created, updated, deleted).

### **Payloads/**
- **CustomerPayload.cs**  
  DTO received from the external API representing a customer record.
- **DataChangeEventPayload.cs**  
  DTO representing raw change events received from the ZeroMQ simulator (`eventType`, `data`).
- **RepoUpdatePayload.cs**  
  Internal strongly-typed representation of update events used inside the backend.

### **Repository/**
- **CustomerRepository.cs**  
  In-memory store for all customers, supporting fast read access and raising change events for indexing.

### **Services/**
- **ApiService.cs**  
  Configures and exposes typed HttpClient instances used to contact the external API.
- **CustomerService.cs**  
  Performs paginated HTTP calls to fetch customer data from the simulator’s `/customers` endpoint.
- **SearchService.cs**  
  Orchestrates Lucene search queries and maps results back into full customer entities.
