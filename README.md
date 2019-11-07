# Tellurian.Trains.Repositories.Xpln
This repository contains source code for a package for reading and validating XPLN (.ods) files.
## XPLN
XPLN is a model railway schedulling tool built on Open Office as an Open Document Spreadsheet, hence the .ods file type. 
With this tool most aspect of model railway scheduling can be made. 
Therefore it is essential that XPLN-documents can be imported into *Tellurian.Trains* database.
## The package
The package reads and validates the following parts of an XPLN file:
* Stations and tracks
* Stretches and lines
* Trains with station times and notes
* Loco schedules
* Trainset schedules
* Duties (also called jobs)

Unlike databases, spreadsheed files like XPLN-files cannot guarantee consistent data. 
In XPLN, a user has the option to run macros to help achieve consistens, 
but any cell can be modified without automatic check of consistence.
Therefore a rigoruios validation is required before it is possible to import XPLN-files into a database.
Validation is performed in two stages:
* First the referential integrity of the XPLN-document is checked to verify that it is consistent. 
Errors found in this stage have to be fixed in the XPLN-document.
* Secondly a number of possible scheduling conflicts are checked. 
Warnings found in this stage can be fixed either in XPLN or in the database.

The first stage is handled in this component, while the second stage is handled in the package *Tellurian.Trains.Models.Planning*. 
