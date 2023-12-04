[![Hack Together: The Great .NET 8 Hack](https://img.shields.io/badge/Microsoft-Hack--Together--.NET8-512BD4?style=for-the-badge&logo=microsoft)](https://github.com/microsoft/hack-together-dotnet)


# Digital Twin Docs

A web application that effortlessly creates a digital twin documentation environment for your company.

Using a company name and a brief description, this project will generate a page with internal documentation to be used by employees and contractors. Every page might include links to other valuable topics, allowing you to dive deeper into specific subjects and potentially generate an infinite amount of information!

## Technologies

Some cool technologies used in this project:
- .NET 8.0
- Aspire ✨
- Minimal APIs
- Native AOT
- Blazor
- Feature Management
- OpenAI/ChatGPT
- Redis Caching
- Blob Storage

## Sequence chart

```mermaid
sequenceDiagram
actor U as User
participant F as Blazor FrontEnd
participant A as Asp.net Api
participant C as Redis Cache
participant B as Blob Storage
participant AI as OpenAi ChatGpt

U->>+F: Visit webpage
F->>U: Retrieve static content
F->>+A: Get markdown

opt Caching
note over A,C: Feature flag for caching is `true`
A->>C: Get markdown
C-->>A: Retrieve cached data
end

opt Persistent Storage
note over A,B: Feature flag for persistent storage is `true`
A->>B: Get blob
B-->>A: Retrieve blob
end

opt Generate
note over A,AI: No alternative has succeeded for retrieving the markown
A->>AI: Generate markdown
AI->>A: Retrieve markdown
A-->>A: Enhance markdown
end

opt Caching
note over A,C: Markdown was not cached
note over A,C: Feature flag for caching is `true`
A-->>C: Set markdown
end

opt Persistent Storage
note over A,B: Markdown was newly generated
note over A,B: Feature flag for persistent storage is `true`
A-->>B: Upload markdown as blob
end

A->>-F: Retrieve markdown
F->>F:Convert to html
F->>-U:Return streamed content
```

## How to use

A short description on how to quickly get up and running after downloading the project.

Requirements:
- Visual Studio 17.9 
- Aspire compontent
- Docker Desktop
- OpenAi account
    - Make note of the api key and uri
- Azure Storage Account[^1]
    - A container named `Markdown`
    - Make note of the access key
- Update all `appsettings.json` files with your information
    - Feature flag `Cache` allows you to cache results to a Redis cache
    - Feature flag `PersistentStorage` allows you to store results to an Azure Blob storage

[^1]: Only required when you use the `PersistentStorage` flag.
