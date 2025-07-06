# FundaAssignment

This repository is used to showcase my ability to program for a medior .Net developer position at Funda.

## Getting Started

To use this project, you have two options, running the project locally or using the Docker image.

### Running the project locally
1. Download the latest release from the [releases page](https://github.com/noleu/FundaAssignment/releases/), specific for your platform.
2. Set the environment variable `FUNDA_API_KEY` to your Funda API key.
3. Run the application using the command line:
   - For Windows: `FundaAssignment.exe`
   - For Linux: `./FundaAssignment`
4. The applicaton will fetch the data from the funda API, print the results to the console, and save them to files in the output  directory

### Using the Docker image

1. Download the source code from the repository.
2. Create an override file `docker-compose.override.yml` in the root directory of the project.
3. Add the following content to the override file:
   ```yaml
    services:
      fundaassignment:
        extends:
          service: 
              fundaassignment
          file: compose.yaml
        environment:
          FUNDA_API_KEY: your_funda_api_key 
   ```
   Replace `your_funda_api_key` with your actual Funda API key.
4. Run the following command to build and run the Docker container:
   ```bash
   docker-compose up --build
   ```
5. The application will fetch the data from the Funda API, print the results to the console, and save them to files in the output directory.
6. The output directory is mounted to the host machine, so you can access the files directly from your host machine.

### Output directory

The output directory contains the files deescribed in the table below. For each file a json and a csv file is generated.

| File Name               | Search  | Description                                                                                                                             |
|-------------------------|---------|-----------------------------------------------------------------------------------------------------------------------------------------|
| amsterdam_overall       | First   | Contains the brokers with the 10 most listings on Funda. In the categories purchase (koop) and rent (huur) combined.                    |
| amsterdam_purchase_only | First   | Contains the brokers with the 10 most listings on Funda to purchase an object in Amsterdam. The broker is only top 10 in this category. |
| amsterdam_rent_only     | First   | Contains the brokers with the 10 most listings on Funda to rent an object in Amsterdam. The broker is only top 10 in this category.     |
| garden_overall          | Second  | Contains the brokers with the 10 most offers to buy a garden in amsterdam. This includes only offers to purchase a garden, not to rent. |

## Design decisions

In this project on several occasions I had to choose between different approaches. Below I will describe the decisions I'm aware that there are other approaches that could have been used.

### Architecture

I have chosen to split the application into three main components, each with its own responsibility:
1. **Extraction client**: This component is responsible for fetching the data from the Funda API. It handles the HTTP requests and responses, and provides a simple interface to get the data.
2. **Transformation logic**: This component is responsible for transforming the data from the Funda API into the desired format. It takes the data from the extraction client and transforms it into a list of brokers with their listings.
3. **Output writer**: This component is responsible for writing the transformed data to files. It takes the list of brokers and writes it to a CSV and JSON file.

Each component could require a different behaviour and therefore a different implementation. 
The extraction could be done from a database, a file, etc.. The transformation logic search for the top 20 or bottom 20 brokers.
The output writer could write the data to a database, send it to a message queue, or require a different file format.
This architecture allows relatively easily to adapt to these different requirements. 
Hiding the implementation details of each component behind an interface would create to more boilerplate code than useful for the scope of this assignment.

### Environment variables

I have chosen to use environment variables to store the Funda API key. Other options include using a configuration file or a secrets manager, or CLI arguments. 
I chose environment variables because they are easy to use and do not require additional dependencies, and avoid the risk of accidentally committing sensitive information to the repository.
A secret manager would have been my preferred option for a production environment, but would be overengineering for this project.

### Rate limiting

As the Funda API has a rate limit, this need to be dealt with at client side. I have chosen the naive or simple approach of waiting for the next request to be allowed.
Simply, as it is working, and it is easy to implement, compared to the more complex approach of implementing a HTTP handler. 

### Pagination

The funda API offers a pagination feature. Due to manual testing I have discovered that the API has fixed maximum page size of 25. 
The amount of pages varies per page size, even when the page size is larger than 25. Therefore is the number of pages not reliable parameter to use for pagination.
Therefore I have I decided to check if a next page is available by checking the `VolgendeUrl' property in the response, in addition to the maximum number of pagees.

### Testing

I have chosen to only test the transformation logic of the application, as it provides the most value for the time invested. 
The other two main components, the extraction client and the output writer are easier to test manually. 
This indicates that my code is testable.

## Use of AI

I have used the auto completion feature of GitHub Copilot to help me write the code. 
I have used it to generate code snippets for logic that I have already thought about, and adapted the generated code to my needs.
I have used it as it is a tool that I am familiar with and it helps me to write code faster, and writes boilerplate code for me.
Further do I have used the chat version of Copilot, to generate the records to parse the json response from the API.
I have used the agent to generate the CI/CD pipeline, as the pipeline is a very nice thing to have, but not the main focus of this project.
Used it for debug the issue of having different address in the http client and the get request, which caused file not found error. 
I have used copilot here, as solving this issue requires slightly advanced knowledge of the HTTP client and how it works, which I do not have and is easier to obtain via an LLM.