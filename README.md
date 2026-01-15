# Console App using Docker and xUnit

This project is a C# console application built with .NET 9 that validates Swedish personal identity numbers (*personnummer*). The application verifies format, date validity, and the control digit according to Swedish rules, and is covered by automated unit tests using xUnit.

## Requirements

* .NET SDK 9.0 or later
* Docker Desktop (for containerization)
* GitHub account (for CI/CD)

## Swedish Personal Identity Number Rules

A Swedish personal identity number consists of 10 or 12 digits.

* **Format:** `YYMMDD-XXXX`, `YYMMDD+XXXX`, `YYYYMMDDXXXX` or `YYYYMMDD-XXXX`.
* **Date Validation:** The first part must represent a valid date.
* **Century Marker:** A hyphen (`-`) is used by default. If a person is 100 years or older, a plus sign (`+`) is used to separate the date from the last four digits.
* **The Luhn Algorithm:** The 10th digit (the last one) is a checksum calculated using the Luhn algorithm on the birth date (YYMMDD) and the first three digits of the birth number.
* **Gender:** The second to last digit indicates gender: even for females and odd for males.

## Application Validation Logic

The application performs the following steps to ensure validity:

1. **Normalization:** Removes non-digit characters (except `+` or `-`) and handles both 10- and 12-digit formats.
2. **Century Resolution:** Uses the separator (`-` or `+`) to determine the correct century for 10-digit inputs.
3. **Date Verification:** Uses `DateTime.TryParseExact` to ensure the date exists (e.g., rejecting February 30th).
4. **Luhn Calculation:** Implements the "Modulus 10" method. It multiplies the digits by 2 and 1 alternately, sums the results (splitting double digits), and verifies that the sum is divisible by 10.
5. **Output:** Returns a normalized `YYYYMMDD-XXXX` string and identifies the person's gender.

## Continuous Integration (GitHub Actions)

This project uses **GitHub Actions** to automate the development workflow:

* **Automated Testing:** On every `push` or `pull_request` to the `main` branch, the pipeline automatically:
* Sets up the .NET 9 environment.
* Restores NuGet dependencies.
* Builds the solution.
* Runs all xUnit tests.


* **Manual Trigger:** The workflow can also be started manually via `workflow_dispatch`.

## Docker Containerization

The application is fully containerized using Docker:

* **Dockerfile:** Uses a multi-stage build (SDK for building and Runtime for a lightweight final image).
* **Automated Publishing:** Once tests pass on the `main` branch, GitHub Actions builds the Docker image and pushes it to **DockerHub**.

### Running with Docker

To run the application via Docker, you must use the interactive terminal flags (`-it`), otherwise the application will loop due to the missing input stream.

**1. Pull the image from DockerHub:**

```bash
docker pull feighty7/personnummer-kontroll:latest

```

**2. Run the container interactively:**

```bash
docker run -it feighty7/personnummer-kontroll:latest

```

## Local Development and Testing

### Running Locally

1. Clone the repository: `git clone https://github.com/tdunca/CI_CD_Group_5.git`
2. Navigate to the project folder.
3. Run the app: `dotnet run --project CI_CD_Group_5`

### Running Tests

To execute the xUnit test suite:

```bash
dotnet test

```

Or use the **Test Explorer** in Visual Studio.