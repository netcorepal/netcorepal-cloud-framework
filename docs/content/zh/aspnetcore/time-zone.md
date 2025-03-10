# 时区处理

要使系统在不同的时区中正确运行，需要考虑时区的处理，这里列出一些原则来确保时区处理的正确性。

+ 使用`DateTimeOffset`代替`DateTime`，则可以规避应用程序时区环境不一致的问题;
+ 使用`DateTimeOffset.UtcNow`代替`DateTime.Now`;
+ 在数据库中存储UTC时间，对于MySQL数据库，`DateTimeOffset`会被自动转为UTC时间存储;
+ 不依赖数据库生成时间，所有时间的生成均由应用程序负责，则可以规避数据库服务器时区不一致的问题;