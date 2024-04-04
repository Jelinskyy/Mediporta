# Mediporta

Mediporta interview project. .Net 8 web api connected with StackOverflow api
## Installation & Deployment

1. Select directory

2. Clone the project

```bash
git clone https://github.com/Jelinskyy/Mediporta
```

3. Go to the project directory

```bash
cd .\Mediporta
```

4. Run docker

```bash
docker compose up
```
## Documentation

[localhost:5000/swagger](http://localhost:5000/swagger/index.html)


## API Reference

#### Get all items

```http
  GET /api/tag
```

#### Fetch tags from SO api and store to db  

```http
  GET /api/tag/fetch
```
## Running Tests

To run tests, run the following command inside project directory

```bash
  dotnet test
```


## Authors

- Aleksander Jeliński 
