# Lab Report 2: OOP Concepts

## 1. Objective
* To implement and demonstrate Object-Oriented Programming principles using C# and .NET Core
* To understand practical applications of encapsulation, inheritance, polymorphism, and abstraction

## 2. Tools
* Visual Studio 2022
* .NET 8.0 Framework/Core
* C# programming language

## 3. Concepts and Implementation

### 3.1 Encapsulation
Encapsulation is implemented by hiding the internal state and requiring all interaction to be performed through an object's methods.

```csharp
public class User
{
    // Private fields - data hiding
    private string _username;
    private string _password;

    // Public properties with controlled access
    public string Username
    {
        get { return _username; }
        set { _username = value; }
    }

    // Password with extra validation
    public string Password
    {
        get { return "********"; } // Never return actual password
        set 
        { 
            if (value.Length >= 8)
                _password = value;
            else
                throw new ArgumentException("Password must be at least 8 characters");
        }
    }

    // Public method that uses the encapsulated data
    public bool Authenticate(string providedPassword)
    {
        return _password == providedPassword;
    }
}
```

### 3.2 Inheritance
Inheritance allows creating derived classes that inherit, extend, or modify the behavior of base classes.

```csharp
// Base class
public class Task
{
    public int TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }

    public virtual string GetTaskInfo()
    {
        return $"Task {TaskId}: {Title} - Due: {DueDate.ToShortDateString()}";
    }
}

// Derived class
public class PriorityTask : Task
{
    public int Priority { get; set; } // 1-5, with 5 being highest

    // Override base class method
    public override string GetTaskInfo()
    {
        return $"Priority {Priority} - " + base.GetTaskInfo();
    }
}
```

### 3.3 Polymorphism
Polymorphism allows objects of different classes to be treated as objects of a common base class.

```csharp
public class TaskManager
{
    public void DisplayTaskInfo(Task task)
    {
        // This method works with any class derived from Task
        Console.WriteLine(task.GetTaskInfo());
        
        // Output differs depending on the actual type of task passed
        // Shows polymorphic behavior
    }

    public void DemonstratePolymorphism()
    {
        // Create instances of base and derived classes
        Task regularTask = new Task
        {
            TaskId = 1,
            Title = "Regular task",
            DueDate = DateTime.Now.AddDays(1)
        };
        
        PriorityTask priorityTask = new PriorityTask
        {
            TaskId = 2,
            Title = "Important task",
            DueDate = DateTime.Now.AddDays(1),
            Priority = 5
        };
        
        // Same method handles different objects polymorphically
        DisplayTaskInfo(regularTask);  // Output: Task 1: Regular task - Due: [date]
        DisplayTaskInfo(priorityTask); // Output: Priority 5 - Task 2: Important task - Due: [date]
    }
}
```

### 3.4 Abstraction
Abstraction focuses on hiding complex implementation details and showing only necessary features.

```csharp
// Abstract base class
public abstract class Notification
{
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Abstract method to be implemented by derived classes
    public abstract void Send();
    
    // Concrete method shared by all notification types
    public void LogNotification()
    {
        Console.WriteLine($"Notification logged at {DateTime.Now}: {Message}");
    }
}

// Concrete implementation
public class EmailNotification : Notification
{
    public string EmailAddress { get; set; }
    
    // Implementation of abstract method
    public override void Send()
    {
        // In a real app, this would use an email service
        Console.WriteLine($"Email sent to {EmailAddress}: {Message}");
    }
}

// Another concrete implementation
public class SMSNotification : Notification
{
    public string PhoneNumber { get; set; }
    
    public override void Send()
    {
        Console.WriteLine($"SMS sent to {PhoneNumber}: {Message}");
    }
}
```

### 3.5 Interfaces
Interfaces define a contract that implementing classes must follow.

```csharp
public interface IStorable
{
    void Save();
    void Load(int id);
    void Delete(int id);
}

public class TaskRepository : IStorable
{
    public void Save()
    {
        Console.WriteLine("Task saved to database");
    }
    
    public void Load(int id)
    {
        Console.WriteLine($"Loading task with ID: {id}");
    }
    
    public void Delete(int id)
    {
        Console.WriteLine($"Deleting task with ID: {id}");
    }
}
```

## 4. Output

```
Demonstration Output:
Task 1: Regular task - Due: 5/28/2025
Priority 5 - Task 2: Important task - Due: 5/28/2025
Email sent to user@example.com: Your task is due tomorrow
SMS sent to +1234567890: Your task is due tomorrow
Task saved to database
Loading task with ID: 1
Deleting task with ID: 1
```

## 5. Conclusion

* OOP principles provide powerful tools for organizing code in a maintainable and reusable way
* Encapsulation allows for data hiding and controlled access, enhancing security and maintainability
* Inheritance enables code reuse and establishing hierarchical relationships between classes
* Polymorphism helps create flexible and extensible applications by treating different objects through a common interface
* Abstraction reduces complexity by hiding implementation details and exposing only what's necessary
* Interfaces provide a way to establish contracts that classes must follow, enabling loose coupling

These OOP concepts are fundamental to modern software development and are extensively used in real-world applications like task management systems, e-commerce platforms, and enterprise software to create modular, maintainable, and scalable code.
