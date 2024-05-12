# Task 4 - Hazelcast Message Queue
### Framework: Minimal API C# .NET 7.0

Author: [Tymur Krasnianskyi](https://github.com/trlumph/)

## 1: Install Hazelcast (OSX)
```bash
brew install hazelcast@5.3.6
brew install hazelcast-management-center@5.3.3
```

## 2: Starting a Hazelcast Cluster
```bash
hz-start
```
![images/node.png](images/node.png)
![images/cluster_running.png](images/cluster_running.png)

## 3: Logging Service
Update the `launchSettings.json` file to include the following environment variables:
- `applicationUrl` - the URL of the service
- `HAZELCAST_NODE` - the address of the Hazelcast node

```json
{
  "profiles": {
    "LoggingService1": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5064",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "HAZELCAST_NODE": "localhost:5701"
      }
    },
    "LoggingService2": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5065",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "HAZELCAST_NODE": "localhost:5702"
      }
    },
    "LoggingService3": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5066",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "HAZELCAST_NODE": "localhost:5703"
      }
    }
  }
}
```
## 4: Messages Service
Update the `launchSettings.json` file to include the following environment variables:
- `applicationUrl` - the URL of the service
```json
{
  "profiles": {
    "MessagesService1": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5074",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "HAZELCAST_NODE": "localhost:5701"
      }
    },
    "MessagesService2": {
      "commandName": "Project",
      "launchBrowser": false,
      "applicationUrl": "http://localhost:5075",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development",
        "HAZELCAST_NODE": "localhost:5702"
      }
    }
  }
}

```
## 5: Running the Services
![images/workflow.png](images/workflow.png)
![images/running_services.png](images/running_services.png)

## 6: Testing the Services
### Post 10 messages
![images/post_message.png](images/post_message.png)

Logging Services:

![images/node1_messages.png](images/node1_messages.png)
![images/node2_messages.png](images/node2_messages.png)
![images/node3_messages.png](images/node3_messages.png)

Messages Services:
![images/messages_service1.png](images/messages_service1.png)
![images/messages_service2.png](images/messages_service2.png)

### Get all messages
*Note: The result format is "`LoggingServiceMessages\nMessagesServiceMessages`"*

From MessagesService 1:

![images/all_messages1.png](images/all_messages1.png)

From MessagesService 2:

![images/all_messages2.png](images/all_messages2.png)

### Shutdown some of the logging nodes
![images/node3_shutdown.png](images/node3_shutdown.png)

The messages are still available in the cluster.

![images/get_after_node_shutdown.png](images/get_after_node_shutdown.png)

![images/node2_shutdown.png](images/node2_shutdown.png)

The messages are still available in the cluster.

![images/get_after_node_shutdown.png](images/get_after_node_shutdown.png)

