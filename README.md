# Schedule Importers

**This repository contains features to validate and import schedule data into an object model.
The objetc model can then be mapped to storage in databases or files.**

> NOTE: This software only reads data into an object model in menory and has no logic for storing data. 
> That have to be implemented elsewhere. This separation of reading and writing is flexible in choosing storage format, 

## Output
This project produces four NuGet packages:
- **TimetablePlanning.Importers.Interfaces** defining import operations
- **TimetablePlanning.Importers.Model** defining the object model.
- **TimetablePlanning.Importers.Access** with logic to read the prototype's Access database.
- **TimetablePlanning.Importers.Xpln** with logic to read XPLN .ODS-files.

## Access Importer
Validates and imports timetable data from the [timetable prototype app](https://github.com/fjallemark/TimetablePlanningApp).
It us currently only experimental and incomplete.

## XPLN Importer
Validates and mports .ODS-files containing XPLN planning data.

### XPLN
XPLN is the defacto tool withing the FREMO community to 
create model railway schedules and printed media for module meetings.
It is developed based on *OpenOffice Calc*, with scripting and forms. 
With this tool most aspect of model railway scheduling can be made. 

Unlike databases, spreadsheed files like XPLN-files cannot guarantee consistent data. 
In XPLN, a user has the option to run macros to help achieve consistensy, 
but any cell can be modified without automatic check of consistence.
Because it lacks the data integrity of a real database, it requires users to
follow a strict workflow to not end up with inconsistent data.

Therefore it is essential that XPLN-documents can be read and validated for formal data consistency
before it can be imported into a database.

### What data is read?
The package reads and validates the following parts of an XPLN file:
* **Station Track** Stations and tracks
* **Routes** Stretches and lines
* **Train** Trains with station times and notes, loco and trainset schedules, and jobs.

The *wheel* and *group* tags are currenly not read.
- *Wheels* denotes train length in axles. It will be added in a forthcoming release.
- *Group* is only for internal purposes in XPLN.

### Validation of XPLN-files
A rigoruios validation is required before it is possible to import XPLN-files into a database.
Validation is performed in two phases:
* In the first phase, the referential integrity of the XPLN-document is checked to verify that it is consistent. 
This means that all references between objects in the worksheets must be valid.
Errors found in this stage must be fixed in the XPLN-document.
* When the referntial integrity is ok, the second validation phase checks for possible scheduling conflicts. 
Warnings found in this stage can be fixed either in XPLN or later in the forthcoming online planning application.

### Multiple language support
A lot of effort has been made to have descriptive validation messages.
Errors found in the data integrity phase also displays the row number in the XPLN-file when the error is detected.
Validation messages are currently in English, German, Danish, Norwegian and Swedish.

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




