using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// --------------------------------------------------------------------------------------
// Класс Guard (ЛР-1: Проверка контрактов)
// --------------------------------------------------------------------------------------
public static class Guard
{
    public static void Requires(bool condition, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message);
        }
    }
}

[TestClass]
public class GuardTests
{
    [TestMethod]
    public void Requires_ValidCondition_DoesNotThrow()
    {
        // ARRANGE
        bool isValid = true;

        // ACT & ASSERT
        // Утверждаем, что исключение не будет выброшено
        Guard.Requires(isValid, "Ошибка!");
    }

    [TestMethod]
    public void Requires_InvalidCondition_ThrowsArgumentException()
    {
        // ARRANGE
        bool isValid = false;

        // ACT & ASSERT
        // Утверждаем, что будет выброшено исключение ArgumentException
        var exception = Assert.ThrowsException<ArgumentException>(() =>
            Guard.Requires(isValid, "Предусловие не выполнено: поле пустое."));

        Assert.IsTrue(exception.Message.Contains("Предусловие не выполнено: поле пустое."));
    }
}

// --------------------------------------------------------------------------------------
// Вспомогательные классы для тестов (модели)
// --------------------------------------------------------------------------------------
public enum TagFilterMode { Not, Or, And }
public class TagItem { public string Name { get; set; } }
public class WorkFilter
{
    public string TypeName { get; set; } = string.Empty;
    public double RatingValue { get; set; } = 0;
    public TagFilterMode TagMode { get; set; } = TagFilterMode.Or;
    public IEnumerable<TagItem> Tags { get; set; } = new List<TagItem>();
}


// --------------------------------------------------------------------------------------
// Класс WorkFilter (Работа с данными)
// --------------------------------------------------------------------------------------
[TestClass]
public class WorkFilterTests
{
    [TestMethod]
    public void Constructor_InitializesDefaultValues()
    {
        // ARRANGE & ACT
        var filter = new WorkFilter();

        // ASSERT
        // Проверяем, что по умолчанию поля инициализированы корректно
        Assert.AreEqual(string.Empty, filter.TypeName);
        Assert.AreEqual(TagFilterMode.Or, filter.TagMode);
        Assert.IsFalse(filter.Tags == null);
        Assert.AreEqual(0, filter.RatingValue);
    }
}

// --------------------------------------------------------------------------------------
// Класс Parser (ЛР-2: Логика WP)
// --------------------------------------------------------------------------------------
public class Parser
{
    // Метод для правила присваивания: wp(x := e, R) = R[x <- e]
    public string ApplyAssignment(string variable, string expression, string postcondition)
    {
        // В реальном коде была бы сложная логика парсинга, здесь - простая замена
        return postcondition.Replace(variable, $"({expression})");
    }

    // Метод для правила последовательности: wp(S1; S2, R) = wp(S1, wp(S2, R))
    // Для демонстрации используется список операций присваивания в обратном порядке
    public string ApplySequence(List<(string variable, string expression)> statements, string finalPostcondition)
    {
        string currentWp = finalPostcondition;
        // Идем с конца (от последнего оператора к первому)
        foreach (var statement in statements)
        {
            // Используем ApplyAssignment для вычисления wp(Si, wp_after_Si)
            currentWp = ApplyAssignment(statement.variable, statement.expression, currentWp);
        }
        return currentWp;
    }
}

[TestClass]
public class ParserTests
{
    [TestMethod]
    public void ApplyAssignment_SimpleSubstitution()
    {
        // ARRANGE
        var parser = new Parser();
        string variable = "x";
        string expression = "y + 1";
        string postcondition = "x > 100";

        // ACT
        string resultWp = parser.ApplyAssignment(variable, expression, postcondition);

        // ASSERT
        Assert.AreEqual("(y + 1) > 100", resultWp);
    }

    [TestMethod]
    public void ApplyAssignment_WithArithmetic()
    {
        // ARRANGE
        var parser = new Parser();
        string variable = "res";
        string expression = "res + a[k]";
        string postcondition = "res = Σ a[i] ∧ k = j";

        // ACT
        string resultWp = parser.ApplyAssignment(variable, expression, postcondition);

        // ASSERT
        // (res + a[k]) = Σ a[i] ∧ k = j
        Assert.AreEqual("(res + a[k]) = Σ a[i] ∧ k = j", resultWp);
    }

    [TestMethod]
    public void ApplySequence_TwoAssignments_CorrectWp()
    {
        // ARRANGE: Программа: S1: y := x + 1; S2: z := y * 2;
        // Постусловие R: z > 10
        // Ожидаемый wp: (x + 1) * 2 > 10, т.е. ((x + 1) * 2) > 10

        var parser = new Parser();
        string finalPostcondition = "z > 10";

        // Порядок важен: последний оператор идет первым в списке для реверсивной обработки
        var statements = new List<(string variable, string expression)>
        {
            ("z", "y * 2"),  // S2: z := y * 2
            ("y", "x + 1")   // S1: y := x + 1
        };

        // ACT
        string resultWp = parser.ApplySequence(statements, finalPostcondition);

        // ASSERT
        // Ожидаемый результат после S2: (y * 2) > 10
        // Ожидаемый результат после S1: ((x + 1) * 2) > 10
        Assert.AreEqual("((x + 1) * 2) > 10", resultWp);
    }
}

// --------------------------------------------------------------------------------------
// Класс TruthTableGenerator (ЛР-4: Логические функции)
// --------------------------------------------------------------------------------------
public class TruthTableResult { public int[] Inputs { get; set; } public bool Output { get; set; } }

public class TruthTableGenerator
{
    public TruthTableResult[] GenerateTable(int numVariables, Func<bool[], bool> function)
    {
        int numRows = 1 << numVariables;
        var results = new TruthTableResult[numRows];
        for (int i = 0; i < numRows; i++)
        {
            var inputs = new bool[numVariables];
            for (int j = 0; j < numVariables; j++)
            {
                // Порядок битов для классической таблицы (A, B) -> (00, 01, 10, 11)
                inputs[j] = (i & (1 << j)) != 0;
            }
            results[i] = new TruthTableResult
            {
                Inputs = Array.ConvertAll(inputs, b => b ? 1 : 0),
                Output = function(inputs)
            };
        }
        return results;
    }
}

[TestClass]
public class TruthTableGeneratorTests
{
    [TestMethod]
    public void GenerateTable_For_AND_Function_CorrectOutput()
    {
        // ARRANGE
        var generator = new TruthTableGenerator();
        int numVariables = 2;
        // Функция: x1 AND x2 (inputs[0] & inputs[1])
        Func<bool[], bool> andFunction = inputs => inputs[0] && inputs[1];

        // ACT
        var table = generator.GenerateTable(numVariables, andFunction);

        // ASSERT
        Assert.AreEqual(4, table.Length);

        // 00 -> False (i=0)
        Assert.IsFalse(table[0].Output);
        // 10 -> False (i=1) -> inputs[0]=True, inputs[1]=False
        Assert.IsFalse(table[1].Output);
        // 01 -> False (i=2) -> inputs[0]=False, inputs[1]=True
        Assert.IsFalse(table[2].Output);
        // 11 -> True (i=3) -> inputs[0]=True, inputs[1]=True
        Assert.IsTrue(table[3].Output);
    }

    [TestMethod]
    public void GenerateTable_For_OR_Function_CorrectOutput()
    {
        // ARRANGE
        var generator = new TruthTableGenerator();
        int numVariables = 2;
        // Функция: x1 OR x2 (inputs[0] | inputs[1])
        Func<bool[], bool> orFunction = inputs => inputs[0] || inputs[1];

        // ACT
        var table = generator.GenerateTable(numVariables, orFunction);

        // ASSERT
        Assert.AreEqual(4, table.Length);

        // 00 -> False (i=0)
        Assert.IsFalse(table[0].Output);
        // 10 -> True (i=1)
        Assert.IsTrue(table[1].Output);
        // 01 -> True (i=2)
        Assert.IsTrue(table[2].Output);
        // 11 -> True (i=3)
        Assert.IsTrue(table[3].Output);
    }
}