# Akka.NET Triathlon Race
The main goal of this repo is to present how the actor-model pattern helps on leveraging the multithreading programming with no need to manage directly the concurrency related to the state change or accessing shared resources.

The framework used to implement the actor-model pattern is [Akka.NET](http://getakka.net).

## Dependencies
- [dotnetcore 3.1 sdk](https://dotnet.microsoft.com/download/dotnet-core/3.1) or [docker](https://www.docker.com/get-started). 
- [akka 1.3.15](https://www.nuget.org/packages/Akka/1.3.15)

## Reference
I used as starting point for the software architecture and the code scaffolding [Akka.NET Traffic Control Sample](https://github.com/EdwinVW/akka-net-traffic-control). On youtube you can find also the [video presentation](https://www.youtube.com/watch?v=6jh6veNLHtk&list=PL0I4QCivrTJBzFi4wEDZJPl13uuFnIK8o&index=38&t=0s) for that sample.

## Run the sample
```
cd src/Host
dotnet run
```
