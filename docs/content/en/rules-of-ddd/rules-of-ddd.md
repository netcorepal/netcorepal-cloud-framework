# Rules of Domain Driven Design

## Rules of Domain Driven Design

### No cross-domain allowed:
+ The change of entity fields must be operated by the entity's own methods.
+ There should be no direct or indirect reference relationship between aggregate roots.
+ Each query should not associate multiple aggregates.

### Avoid reuse:
+ Create an API for each frontend scenario.
+ Create separate input and output entities (RequestDto, ResponseDto) for each API.
+ Create separate commands for each operation.


## Rules of Domain Driven Design

### No cross-domain allowed:
+ The change of entity fields must be operated by the entity's own methods.
+ There should be no direct or indirect reference relationship between aggregate roots.
+ Each query should not associate multiple aggregates.

### Avoid reuse:
+ Create an API for each frontend scenario.
+ Create separate input and output entities (RequestDto, ResponseDto) for each API.
+ Create separate commands for each operation.
