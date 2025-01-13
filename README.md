# GenerateFile & SortTextFile

1. GenerateFile - a program creates a test file of the specified size. 
All parameters for creating a test file are set in appsettings.json

command line:
GenerateFile.exe "c:\testPath\test.txt"


2. SortTextFile - a program for sorting large text files of a certain format.
All parameters for creating a test file are set in appsettings.json


command line:
SortTextFile.exe "c:\Users\Dell\source\repos\3deye\GenerateFile\GenerateFile\bin\Debug\net8.0\output1.txt"


appsettings.json:

- TempDirectory is a directory for temporary files, intermediate files with folders will be created here!!! 
if not specified, the current folder is taken.
All files and subfolders in the directory are deleted during operation

- IsDeleteFile = true - delete temporary files after sorting
- MaxNumberThreads - the number of threads that can be used for parallel file sorting
- LengthBookIndex is the length of the sorting index. Now there are 4 characters, depending on the text in the file, the parameter for the study.