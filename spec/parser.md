# Retrosharp ETL Parsers

## Overview

The Retrosharp ETL parsers are responsible for parsing Retrosheet's datafiles and populating the Retrosharp database with relevant information. The parsers are designed to handle various data formats and ensure that the data is processed accurately and efficiently.

## Design Considerations

Each parser is designed to be idempotent, meaning that the same record should not be duplicated or inserted multiple times. The parsers are also designed to handle incomplete or incompatible data formats, ensuring that the most up-to-date information is stored in the database. Each parser is atomic in nature, meaning that if an unrecoverable error occurs during processing, the database should not be left in an inconsistent state. No partial parses are allowed. Each parser should log and report how many records were added and updated in the database at the end of the parse. Each parser is a NServiceBus saga that processes the datafile in a background task, allowing for efficient processing of large datafiles without blocking the main application thread. The parsers are designed to be modular and extensible, allowing for easy addition of new parsers as needed. Each parser is started by receiving a message from the service bus, which triggers the parsing process. The message is placed onto the bus by an API call from the Retrosharp web application, which allows for easy integration with other systems and applications. The parsers are designed to be flexible and configurable, allowing for customization of the parsing process as needed. Each parser is designed to handle errors gracefully, logging any errors that occur during processing and allowing for easy troubleshooting and debugging. The parsers are designed to be efficient and performant, ensuring that large datafiles can be processed quickly and accurately. Each parser is designed to be maintainable and easy to understand, with clear documentation and well-structured code that follows best practices for software development.

## Acceptance Criteria

1. Each parser should be able to successfully read and process Retrosheet's datafiles, extracting relevant information and populating the appropriate tables in the Retrosharp database.
1. Each parser logs the amount of records added and updated in the database at the end of the parse.
1. Each parser is atomic in nature. If an unrecoverable error occurs during processing, the database should not be left in an inconsistent state. No partial parses!
1. Each parser shall implement idempotent data handling. The same record should not be duplicated or inserted multiple times.
1. Each parser should be able to handle incomplete or incompatible data formats using appropriate methods for each datafile.
1. Each parser should be able to self heal and recover from errors during processing, allowing for continued processing of the datafile without interruption.
	1. Each parser should log any errors that occur during processing, allowing for easy troubleshooting and debugging.
	1. Exponential backoff should be used for retrying failed operations, with a maximum number of retries before the parser fails and logs the error. Use a combination of NServiceBus's built-in retry policies and custom retry logic to handle transient errors and ensure that the parser can recover from temporary issues without failing the entire parsing process.

## Additional Information

1. How to implement a NServiceBus saga for parsing Retrosheet datafiles: [NServiceBus documentation](https://docs.particular.net/nservicebus/sagas/)
1. The Polly .NET library for implementing retry policies and handling transient errors: [Polly](https://www.pollydocs.org/)
