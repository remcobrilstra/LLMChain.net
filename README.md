# LLMChain.net
Simple abstraction for implementing conversational logic and chain-of-thought with different LLM providers, for #DotNet


## Warning
This is a work in progress, and is not intended for production use. 
I Created this purely for fun and to learn more about LLMs and how they can be used in conversational logic.
Feel free to copy modify and/or use this code in any way you see fit, but use at your own risk.


## Getting Started
To get started copy the appsettings.json file and rename it appsettings.local.json. Fill in the values for your LLM provider.

The sample app is a simple console app that demonstrates how to use the library.

Have fun :)


## To Do
- Implement the Anthropic Api
- Refactor the code the make you eyes bleed less
- Implement custom CoT logic, because why not
- Support (LM Studio)[https://lmstudio.ai/] as a provider (although the API is the same as OpenAI's?! so mostly copy-paste?)