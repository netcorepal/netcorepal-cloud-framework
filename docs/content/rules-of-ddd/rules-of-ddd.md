# Rules of Domain Driven Design

## 领域驱动设计的原则

### 不准跨域：
+ 实体字段的变化，必须由实体自己的方法来操作；
+ 聚合根之间不能发生直接或间接的引用关系；
+ 每个查询不能关联多个聚合；

### 不要复用：
+ 为每个前端场景创建一个API；
+ 为每个API创建各自的输入输出实体（RequestDto、ResponseDto）；
+ 为每个操作创建各自的命令（Command）；


## Rules of Domain Driven Design

### No cross-domain allowed:
+ The change of entity fields must be operated by the entity's own methods.
+ There should be no direct or indirect reference relationship between aggregate roots.
+ Each query should not associate multiple aggregates.

### Avoid reuse:
+ Create an API for each frontend scenario.
+ Create separate input and output entities (RequestDto, ResponseDto) for each API.
+ Create separate commands for each operation.
