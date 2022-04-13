# Xpln Validator
**This repository contains source code for a package for reading and validating XPLN (.ods) files.**
> NOTE: This is an experimental software. It is not intended for production use, only
> to demonstrate the issues with importing XPLN files into a database.

#### XPLN
XPLN is a model railway scheduling tool built on *Open Office* spreadsheet, hence the .ods file type. 
With this tool most aspect of model railway scheduling can be made. 
Therefore it is essential that XPLN-documents can be imported into *Tellurian.Trains* database.

#### The package
The package reads and validates the following parts of an XPLN file:
* Stations and tracks
* Stretches and lines
* Trains with station times and notes
* Loco schedules
* Trainset schedules
* Duties (also called jobs)

#### About Spreadsheet Applications
Unlike databases, spreadsheed files like XPLN-files cannot guarantee consistent data. 
In XPLN, a user has the option to run macros to help achieve consistensy, 
but any cell can be modified without automatic check of consistence.

#### Validation of XPLN-files
Therefore a rigoruios validation is required before it is possible to import XPLN-files into a database.
Validation is performed in two phases:
* In the first phanse, the referential integrity of the XPLN-document is checked to verify that it is consistent. 
Errors found in this stage stops import and must be fixed in the XPLN-document.
* When the integrity is validated, the second validation phase checks for possible scheduling conflicts. 
Warnings found in this stage can be fixed either in XPLN or later in the forthcoming online planning application.

The unit tests of this software uses a set of XPLN files from different origins.
All the unit test XPLN-files had some kind of issue that required correction of the XPLN-file 
before it could be succesfully validated. This indicates the problems with spreadsheet solutions. 
