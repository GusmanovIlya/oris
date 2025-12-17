namespace StudentCardsServer;

public sealed record Student(
    int Id,
    string FullName,
    int Age,
    string Group,
    double Gpa
);
