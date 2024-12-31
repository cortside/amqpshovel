[![Build status](https://ci.appveyor.com/api/projects/status/f3tnsngabv2dx0t3?svg=true)](https://ci.appveyor.com/project/cortside/cortside-restapiclient)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=cortside_amqptools&metric=alert_status)](https://sonarcloud.io/dashboard?id=cortside_amqptools)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=cortside_amqptools&metric=coverage)](https://sonarcloud.io/dashboard?id=cortside_amqptools)

# amqptools

CLI tools for interacting with service bus queues.

## Help

Will show available commands

```powershell
./AmqpTools.exe --help
```

### Command help

Will show available and required options

```powershell
./AmqpTools.exe shovel --help
```

## Shovel

```powershell
$policyname = "SendListen"
$namespace = "acme.servicebus.windows.net"
$key = "secret=="
$queue = "shoppingcart.queue"

./AmqpTools.exe shovel --queue $queue --namespace $namespace --policyname=$policyname --key=$key
```

## Publish

```powershell
$policyname = "SendListen"
$namespace = "acme.servicebus.windows.net"
$key = "secret=="
$queue = "shoppingcart.queue"
$event = "Acme.ShoppingCartUpdatedEvent"

./AmqpTools.exe publish --queue $queue --namespace $namespace --policyname=$policyname --key=$key --eventtype $event --data '{\"ShoppingCartResourceId\":\"e25d2090-d890-4b8a-a904-5feebf4b6436\"}'

OR

./AmqpTools.exe publish --queue $queue --namespace $namespace --policyname=$policyname --key=$key --eventtype $event --file "event.json"
```

## Queue details

```powershell
$policyname = "SendListen"
$namespace = "acme.servicebus.windows.net"
$key = "secret=="
$queue = "shoppingcart.queue"

./AmqpTools.exe queue --queue $queue --namespace $namespace --policyname=$policyname --key=$key
```
