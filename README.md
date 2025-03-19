# Procare Solutions Technical Challenge  

## Getting Started  
To run this project, install the following:  
- [VSCode (optional)](https://code.visualstudio.com/)  
- [.NET 8.0](https://dotnet.microsoft.com/en-us/download)  
- [C# Dev Kit Extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)  

## Requirements  
This challenge involved improving API request handling with the following enhancements:  
1. Cancel the request and retry if the call takes longer than **750ms**.  
2. Retry requests when encountering an **HTTP 5xx** response. Let **4xx errors** bubble up as exceptions.  
3. Fail after **3 unsuccessful attempts** (due to timeouts or HTTP 5xx errors).  
4. Implement **exponential backoff with jitter** to prevent overwhelming the API.  

## Implementation  
The requested changes primarily affected external API requests, making **`AddressValidationService`** the best place for implementation ([code reference](https://github.com/nicktu12/Procare.AddressValidation.Tester.Original.v2/blob/main/AddressValidationService.cs)).  

I implemented a **retry pattern** using a while loop that tracks attempts and applies conditional logic for retries, including logging for visibility (which would be removed in production). **Backoff and jitter** were handled in a separate private method to prevent excessive simultaneous retries.  

While considering refactoring, I evaluated extracting **5xx and non-5xx error handling** into private methods. However, I found the refactored version less readable, so I chose to keep these checks within **`GetAddressAsync`** for clarity.  

## Testing & AI Usage  
To familiarize myself with the **.NET framework** and this project, I leveraged AI for research and exploring built-in functionality. AI was particularly useful for generating **test scaffolding** (see [testing branch](https://github.com/nicktu12/Procare.AddressValidation.Tester.Original.v2/tree/testing?tab=readme-ov-file)). However, fully implementing a test suite would have required more significant changes to the application structure. Given more time, I would refine this further.  

## Final Thoughts  
I enjoyed working on this challenge and refreshing my **.NET** skills—thanks for the opportunity!  
