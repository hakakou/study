dotnet nswag openapi2csclient `
  /input:https://raw.githubusercontent.com/ddsky/world-news-api-clients/main/world-news-api-openapi-3.json `
  /classname:NewsService `
  /namespace:NewsWeb `
  /output:Data/NewsService.cs