mkdir C:\sipod-example\rabbitmq-data
docker run --rm -d -it --hostname localhost -p 5672:5672 -p 15672:15672 --name rabbitmq rabbitmq:3-management