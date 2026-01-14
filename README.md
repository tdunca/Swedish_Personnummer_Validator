# Console App using docker and xUnit

This project is a C# console application that validates Swedish personal identity numbers
(*personnummer*). The application verifies format, date validity, and the control digit
according to Swedish rules, and is covered by automated unit tests using xUnit.

## Requirements
- .NET SDK 6.0 or later
- Visual Studio (recommended) or .NET CLI

### Info
------- Inkludera även information om svenska regler för personnummer och hur din applikation genomför kontrollen 

## Installing and running 
1. Clone the repository
2. Run
3. Enter a swedish personal identity number in console

### Docker


### Testing
1. Open Test Explorer
2. Click 'Run All' or 'Run Specific Test'

## Swedish personal identity number rules 
- Format: YYMMDD-XXXX or YYYYMMDDXXXX

### Application Validation
1. Cleans input from invalid characters
2. Verifies correct length and supported formats
3. Validates that the date portion representes a real calendar date
4. Calculates and verifies the control digit using the Luhn algorithm
5. Returns a valid or invalid result along with descriptibe message
