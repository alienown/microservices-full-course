# .NET Microservices – Full Course
This repository is an introduction to building microservices using .NET. It is based on course https://github.com/binarythistle/S04E03---.NET-Microservices-Course-.
The application shares information about platforms (e.g. .NET) and its commands (e.g. dotnet run). There are two services - Platform service and Command service.
As a developer with no professional experience in building applications using microservices architecture, this course was my first practical encounter with this topic.
This repo will serve as a base and code lookup for personal projects using microservices that I plan to do in the future. Maybe I will do some updates here too to extend
functionalities introduced in course.

## Used technologies
- .NET 6 Web API
- Entity Framework
- SQL Server
- AutoMapper
- Docker
- Kubernetes
- RabbitMQ (https://www.rabbitmq.com/dotnet.html)
- gRPC (https://www.nuget.org/packages/Grpc.AspNetCore)

## Example data from Platform and Command services
An example of platforms from Platform service `GET` endpoint `/api/platforms`:
```
[
    {
        "id": 1,
        "name": "Dot Net",
        "publisher": "Microsoft",
        "cost": "Free"
    },
    {
        "id": 2,
        "name": "SQL Server Express",
        "publisher": "Microsoft",
        "cost": "Free"
    },
    {
        "id": 3,
        "name": "Kubernetes",
        "publisher": "Cloud Native Computing Foundation",
        "cost": "Free"
    }
]
```
An example command from Command service for platform `Dot Net` (`GET` `/api/c/platforms/1/commands`)
```
[
    {
        "id": 1,
        "howTo": "Build a .net project",
        "commandLine": "dotnet build",
        "platformId": 1
    }
]
```

## Architecture
Platform and Command services can communicate using synchronous (REST API) or asynchronous, event-driven way using message bus.
Ingress nginx controller is used as an API gateway to deliver services to outside clients.
When platform is created in Platform service, it is published to the message bus. As a subscriber, Command service receives the platform published event and
inserts new platform into its own database. In this way both services are keeped in sync. The gRPC is beign run on Command service startup to fetch all platforms
from platform service and insert missing platforms into the database.

![image](https://user-images.githubusercontent.com/47573956/166148968-d63dbda8-fb12-49b4-86e4-8e578fda9702.png)
Figure src: https://www.youtube.com/watch?v=DgVjEo3OGBI&ab_channel=LesJackson

## Usage
To start up services you need to build docker images for platform and command service. Then create deployments for kubernetes.
Additionaly you have to make changes in K8S directory files so that kubernetes deployments point to your docker hub.
```
docker build -t <your docker hub>/platformservice .
docker push <your docker hub>/platformservice
docker build -t <your docker hub>/commandservice .
docker push <your docker hub>/commandservice

kubectl apply -f local-pvc.yaml
kubectl create secret generic mssql --from-literal=SA_PASSWORD="pa55w0rd!"
kubectl apply -f mssql-plan-depl.yaml
kubectl apply -f rabbitmq-depl.yaml
kubectl apply -f platforms-depl.yaml
kubectl apply -f commands-depl.yaml
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.1.1/deploy/static/provider/cloud/deploy.yaml
kubectl apply -f ingress-srv.yaml
```
