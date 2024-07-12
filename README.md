# amqptools

AMQPTools 1.0

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
