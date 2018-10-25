# nextgames
The common chat is placed on the main (Index) page of the application. It can also be called a "wall" in a sense that anyone can leave a message there freely.
The website is deployed with azure (Github is used for deployments): https://ngchat20181025034200.azurewebsites.net/


The application consists of several base components: 
CommonChatHub - signalR hub
  SendMessage
    Allows the user to send a message
  GetMessageHistory -> ICollection<WallMessage>
    Allows to get all history starting from some defined date
  Ping
    Shows server that client is still alive
  OnConnectedAsync
    Triggers a broadcast message immediately showing the new user

HomeController
  Index
    Returns the view with the wall. Shown only to authorized users (all registered users).
  About
  Contact
  Privacy

ServerOnlineNotificationTimer - is a hosted service, broadcasting current states of users (online/offline)

=========
The application utilizes 2 databases: 
SQL database storing users
  Assumption is that in real world, such small service probably will not store its own users but it has to be integrated with some existing database. SQL database represents such database. Users are connected by GUID provided by Identity membership system.
Azure store database storing comments and user states
  This database is Azure Table storage database. 

===========
I want to highlight the displaying of users online state. The obvious solution would be to override "OnConnectedAsync"/"OnDisconnectedAsync" methods and count connections. Despite its common use in examples for asp.net core, this approach is not reliable as in some cases the connection can be interrupted without us knowing. The tcp socket can be stuck for up to half an hour. It also was proven to be unreliable even with simpler cases when user opens several connections.
Therefore, I came up with another approach: storing last timestamp of for all users. Users have to ping the server to be assumed to be online. All other interaction are also written into the database (just the last timestamp) and online users are those users, who interacted with the system (including regular ping) less than predefined number of seconds from the current time (all time is stored in UTC).
Obviously, this approach produces another problem: we do not know when to notify the users about "dead" users (we have to query the database). One of the solutions might be for user to ask list of users each several seconds. It would not scale so I came up with a better approach: having a hosted service that queries the database every N seconds and broadcasts current online users (this can be optimized in such way that timing will depend on the database state). This allows to significantly reduce the number of requests and utilize signalR's capability of sending broadcasted packages.
Timings can be optimized in such way that the system works almost immediately for recently exited users. It can be further configured by editing ping frequency, database query frequency and threshold to consider user to be offline.  
I still use "OnConnectedAsync" method to update the state and broadcast the new state from the Hub directly. It allows to show the new user without any delays.






In this project I used only .NET Core 2 Dependency Injection as it was stated in the task. However, I might have used autofac as it provides possibility to work with fields directly. However, I didn't lack functionality of .NET Core DI during this project.

