This is the `Messenger.Core` project, it holds the core/backend functionality and data models.

The [Helpers](Helpers/) directory holds non-essential functionality - like conversion from DB rows to classes - and is used internally by the [Services](Services/) 
The [Models](Models/) directory holds essential data models like classes for teams and messages or enums for things like notification types
The [Services](Services/) directory holds the core functionality - like creating teams and messages - for interacting with the external database and blob storage
