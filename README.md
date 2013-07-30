#ExpressiveAnnotations - annotation-based conditional validation

[**ExpressiveAnnotations**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations) is a small .NET library which provides annotation-based conditional validation mechanisms. Given implementation of **RequiredIf** and **RequiredIfExpression** attributes allows to forget about imperative way of step-by-step verification of validation conditions in many cases. This in turn results in less amount of code which is also more compacted, since fields validation requirements are applied as metadata, just in the place of such fields declaration.

###What are brief examples of usage?

For sample usages go to [**demo project**](https://github.com/JaroslawWaliszko/ExpressiveAnnotations/tree/master/src/ExpressiveAnnotations.MvcWebSample).

* Simplest, using **RequiredIfAttribute** which *provides conditional attribute to calculate validation result based on related property value*:
    
 ```
[RequiredIf(DependentProperty = "GoAbroad", TargetValue = true)]
public string PassportNumber { get; set; }
```
* More complex, using **RequiredIfExpressionAttribute** which *provides conditional attribute to calculate validation result based on related properties values and relations between them, which are defined in logical expression*:

 ```
[RequiredIfExpression(
        Expression = "{0} || (!{1} && {2})",
        DependentProperties = new[] { "SportType", "SportType", "GoAbroad" },
        TargetValues = new object[] { "Extreme", "None", true },
        ErrorMessage = "Blood type is required if you do extreme sports, or 
                        if you do any type of sport and plan to go abroad.")]
public string BloodType { get; set; }
```
How such an expression should be understood?

 ```SportType == "Extreme" || (SportType != "None" && GoAbroad)```


###How to construct conditional validation attributes?

```
RequiredIfAttribute([string DependentProperty], [object TargetValue], ...)    

    DependentProperty - Field from which runtime value is extracted.    
    TargetValue       - Expected value for dependent field. If runtime value is the same, requirement condition
                        is fulfilled and error message is displayed.
```
```
RequiredIfExpressionAttribute([string Expression], [string DependentProperty], [object TargetValue], ...)

    Expression        - Logical expression based on which requirement condition is calculated. If condition 
                        is fulfilled, error message is displayed. Attribute logic replaces one or more format 
                        items in specific expression string with comparison results of dependent fields and 
                        corresponding target values.                         
                        Available expression tokens are: &&, ||, !, {, }, numbers and whitespaces.
    DependentProperty - Dependent fields from which runtime values are extracted.    
    TargetValue       - Expected values for corresponding dependent fields.
```

Sample `{0} || !{1}` expression evaluation steps:

1. Expression is interpreted as (notice that arrays indexes of related properties and its values are given in expression inside curly parentheses `{}`): 

 ```(DependentProperties[0] == TargetValues[0]) && (DependentProperties[1] != TargetValues[1])```
2. Arrays values are extracted and compared. Boolean computation results are inserted into corresponding brackets, let's say:

 ```(true) || (false)```
3. Such preprocessed expression is now converted from infix notation, to reflect Reverse Polish Notation (RPN) syntax:

 ```true false ||```
4. Finally postfix expression is evaluated to give validation result. Here it is true (condition fulfilled), so error message is risen.

###What is the context behind this implementation? 

Simplicity. Declarative validation is more convenient. Cleaner and more compacted code goes hand in hand with it.

###What is the difference between declarative and imperative programming?

With **declarative** programming, you write logic that expresses what you want, but not necessarily how to achieve it. You declare your desired results, but not the step-by-step.

In our example it's more about metadata, e.g.

```
[RequiredIfExpression(
    Expression = "{0} || (!{1} && {2})",
    DependentProperties = new[] { "SportType", "SportType", "GoAbroad" },
    TargetValues = new object[] {"Extreme", "None", true},
    ErrorMessage = "Blood type is required if you do extreme sports or, 
                    if you do any type of sport and plan to go abroad.")]
public string BloodType { get; set; }
```

With **imperative** programming, you define the control flow of the computation which needs to be done. You tell the compiler what you want, step by step.

If we choose this way instead of model fields decoration, it has negative impact on the complexity of the code. Logic responsible for validation is now implemented somewhere else in our application e.g. inside controllers actions instead of model class itself:
```
    if (!string.IsNullOrEmpty(model.BloodType))
    {
        return View("Success");
    }
    if (model.SportType == "Extreme" || (model.SportType != "None" && model.GoAbroad))
    {
        ModelState.AddModelError("BloodType", "Blood type is required if you do extreme sports or, 
                                               if you do any type of sport and plan to go abroad.");    
    }
    return View("Home", model);
}
```