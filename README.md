
# Utility Attributes and Analyzer for Unity

Some attributes and analyzers I need when working with Unity




## Current Attributes

 ### [MustCallBase]
```
Add this to  your base method to force all the overriding method to call base()
Serverity: Warning
```



## Installation

There're two parts to the repo you need to deal with:

#### The UtilAttribute project 
- Once built would become the dll containing the definition of all the util attributes.
- Put this dll in Assets/Plugins and include it in "Any platform"

#### The UtilAnalyzer project
- Once built would contain the analyzer script to enforce the analyzer rules.
- This dll must be in an Editor folder
- Untick "Validate Reference" and "Any Platform"
- Add the "RoslynAnalyzer" tag to the dll

(Go to relase page for prebuilt dlls)
