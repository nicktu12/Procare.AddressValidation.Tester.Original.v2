# Procare Solutions Technical Challenge

## Getting Started
Download VSCode, .Net 8.0, and C# Dev Kit Extension in VS Code

## Requirements
The requirements of this challenge were to: 
1.  Update the application so that when the call takes longer than 750ms to respond, cancel the request and try again.
2.  Update the application so that when the response is an HTTP 5xx, retry the request. (If it is a 4xx or other failure, just let that exception bubble up.)
3.  If after 3 attempts the calls to the API still fail (either HTTP 5xx or timeout), throw an exception.
4.  Add back-off and jitter.

## Notes
The requested features are dealing with external API requests, so it made the most sense to apply this logic to the [AddressValidationService](). Utilizing the [retry pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/retry) and a while loop based on the number of retried network requests, I was able to create a series of conditions that determine the service's next steps, with some logging output for clarity (these would be likely removed in a production application.) Delay and jitter were added as a private method of this class to assist in preventing overwhelming the API with too many requests, or requests from multiple application instances falling in sync. 

Some code that I considered refactoring was the 5xx and non-5xx error handling. These checks could be abstracted to their over private methods, but I found the refactored solution lacked readability, so I decided to leave these checks within the GetAddressAsync method. 