# XPLN Validator
**This repository contains source code for a package for reading and validating XPLN (.ods) files.**
> NOTE: This is an experimental software. It is not intended for production use, only
> to demonstrate the issues with importing XPLN files into a database.

## XPLN
XPLN is the defacto tool withing the FREMO community
to create model railway schedules and printed media for module meetings.
It is developed based on *OpenOffice Calc*, with scripting and forms. 
With this tool most aspect of model railway scheduling can be made. 

Unlike databases, spreadsheed files like XPLN-files cannot guarantee consistent data. 
In XPLN, a user has the option to run macros to help achieve consistensy, 
but any cell can be modified without automatic check of consistence.
Because it lacks the data integrity of a real database, it requires users to
follow a strict workflow to not end up with inconsistent data.

Therefore it is essential that XPLN-documents can be read and validated for formal data consistency
before it can be imported into *Tellurian.Trains* database.

### Validation of XPLN-files
A rigoruios validation is required before it is possible to import XPLN-files into a database.
Validation is performed in two phases:
* In the first phase, the referential integrity of the XPLN-document is checked to verify that it is consistent. 
This means that all references between objects in the worksheets must be valid.
Errors found in this stage must be fixed in the XPLN-document.
* When the referntial integrity is ok, the second validation phase checks for possible scheduling conflicts. 
Warnings found in this stage can be fixed either in XPLN or later in the forthcoming online planning application.

### The story of reading XPLN-files
XPLN is stored in ODS-files, an *Open Document* format. 
Despite it is open, it is tricky to read. 
- First, I used Excel COM-objects, which make reading dependent having Microsoft Excel installed.
Excel can open ODS-files directly. This was easiest to start with, but not a solution that can run in the cloud or be distributed.
- In the second effort, I found the **ExcelDataReader** package, that removed the dependency of Microsoft Excel.
But it required the ODS-files to be converted to .XLSX before reading. 
Althoug there are free online converters, it forces the user to make a conversion. 
- Finally, I found a 10+ year old codebase for reading ODS-files directly. 
After some tweaking, I made i work. And it had much better performance than the initial Excel-solution.

### How is this Software tested?
Not two planners use XPLN exacly the same way, validating one XPLN-file is'nt enough.
The only way to test is to have a large number of XPLN-files to read an validate.
Therefore, the tests of this software uses a set of XPLN files from different origins.
All of the tested XPLN-files had some kind of data integrity issue that required correction of the XPLN-file 
before it could be succesfully validated. 
This clearly demonstrates the problems with using a spreadsheet for complex data storage.

### The package
The package reads and validates the following parts of an XPLN file:
* Stations and tracks
* Stretches and lines
* Trains with station times and notes
* Loco schedules
* Trainset schedules
* Jobs

The *wheel* and *group* tags are currenly not read. The groups I don't yet understand the meaning of.

#### Multiple language support
A lot of effort has been made to have descriptive validation messages.
Errors found in the data integrity phase also displays the row number in the XPLN-file when the error is detected.
Validation messages are currently in English, German, Danish, Norwegian and Swedish.

#### Future plans
The next step is to offer the XPLN-valdidation as a cloud-service, as part of the [*Module Registry* toolset](https://moduleregistry.azurewebsites.net/Tools).
The purpose is to help planners finding errors, but also to learn more about any weaknesses in the validation.

After that, I see a clear case to be able to import XPLN-plans into the forthcoming cloud-based scheduling database.
This could be a quickstart to utilise the report functionality and features not available in XPLN.
