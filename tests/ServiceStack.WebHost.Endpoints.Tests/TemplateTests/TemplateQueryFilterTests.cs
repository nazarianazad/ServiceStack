﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Templates;
using ServiceStack.Text;
using ServiceStack.VirtualPath;

namespace ServiceStack.WebHost.Endpoints.Tests.TemplateTests
{
    public class TemplateQueryFilterTests
    {
        private static TemplateContext CreateContext(Dictionary<string, object> optionalArgs = null)
        {
            var context = new TemplateContext
            {
                Args =
                {
                    ["numbers"] = new[] { 5, 4, 1, 3, 9, 8, 6, 7, 2, 0 },
                    ["products"] = TemplateQueryData.Products,
                    ["customers"] = TemplateQueryData.Customers,
                    ["digits"] = new[]{ "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" },
                    ["strings"] = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" },
                    ["words"] = new[]{"cherry", "apple", "blueberry"},
                    ["doubles"] = new[]{ 1.7, 2.3, 1.9, 4.1, 2.9 },
                }
            };
            optionalArgs.Each((key, val) => context.Args[key] = val);
            return context.Init();
        }

        [Test]
        public void Linq01() // alternative with clean whitespace sensitive string argument syntax:
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Numbers < 5:
{{ numbers | where('it < 5') | select: { it }\n }}").NormalizeNewLines(), 
                
                Is.EqualTo(@"
Numbers < 5:
4
1
3
2
0
".NormalizeNewLines()));
        }

        [Test]
        public void Linq02() // alternative with clean whitespace sensitive string argument syntax:
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Sold out products:
{{ products 
    | where: it.UnitsInStock = 0 
    | select: { it.productName | raw } is sold out!\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
Sold out products:
Chef Anton's Gumbo Mix is sold out!
Alice Mutton is sold out!
Thüringer Rostbratwurst is sold out!
Gorgonzola Telino is sold out!
Perth Pasties is sold out!
".NormalizeNewLines()));
        }

        [Test]
        public void Linq03()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
In-stock products that cost more than 3.00:
{{ products 
    | where: it.UnitsInStock > 0 and it.UnitPrice > 3 
    | select: { it.productName | raw } is in stock and costs more than 3.00.\n 
}}
").NormalizeNewLines(),
                
                Does.StartWith(@"
In-stock products that cost more than 3.00:
Chai is in stock and costs more than 3.00.
Chang is in stock and costs more than 3.00.
Aniseed Syrup is in stock and costs more than 3.00.
".NormalizeNewLines()));
        }

        [Test]
        public void Linq04()
        {
            var context = CreateContext(new Dictionary<string, object>
            {
                {TemplateConstants.DefaultDateFormat, "yyyy/MM/dd"}
            });
            
            context.VirtualFiles.WriteFile("customer.html", @"
Customer {{ it.CustomerId }} {{ it.CompanyName | raw }}
{{ it.Orders | selectPartial: order }}");

            context.VirtualFiles.WriteFile("order.html", "  Order {{ it.OrderId }}: {{ it.OrderDate | dateFormat }}\n");
            
            Assert.That(context.EvaluateTemplate(@"
{{ customers 
   | where: it.Region = 'WA' 
   | assignTo: waCustomers 
}}
Customers from Washington and their orders:
{{ waCustomers | selectPartial: customer }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Customers from Washington and their orders:

Customer LAZYK Lazy K Kountry Store
  Order 10482: 1997/03/21
  Order 10545: 1997/05/22

Customer TRAIH Trail's Head Gourmet Provisioners
  Order 10574: 1997/06/19
  Order 10577: 1997/06/23
  Order 10822: 1998/01/08
".NormalizeNewLines()));
        }

        [Test]
        public void Linq05()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Short digits:
{{ digits 
   | where: it.Length < index
   | select: The word {it} is shorter than its value.\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Short digits:
The word five is shorter than its value.
The word six is shorter than its value.
The word seven is shorter than its value.
The word eight is shorter than its value.
The word nine is shorter than its value.
".NormalizeNewLines()));
        }

        [Test]
        public void Linq06()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Numbers + 1:
{{ numbers | select: { it | incr }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Numbers + 1:
6
5
2
4
10
9
7
8
3
1
".NormalizeNewLines()));
        }

        [Test]
        public void Linq07()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Product Names:
{{ products | select: { it.ProductName | raw }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Product Names:
Chai
Chang
Aniseed Syrup
Chef Anton's Cajun Seasoning
Chef Anton's Gumbo Mix
".NormalizeNewLines()));
        }

        [Test]
        public void Linq08()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Number strings:
{{ numbers | select: { strings[it] }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Number strings:
five
four
one
three
nine
eight
six
seven
two
zero
".NormalizeNewLines()));
        }

        [Test]
        public void Linq09()
        {
            var context = CreateContext(new Dictionary<string, object>
            {
                {"words", new[]{ "aPPLE", "BlUeBeRrY", "cHeRry" }}
            });
            
            Assert.That(context.EvaluateTemplate(@"
{{ words | select: Uppercase: { it | upper }, Lowercase: { it | lower }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Uppercase: APPLE, Lowercase: apple
Uppercase: BLUEBERRY, Lowercase: blueberry
Uppercase: CHERRY, Lowercase: cherry
".NormalizeNewLines()));
        }

        [Test]
        public void Linq10()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
{{ numbers | select: The digit { strings[it] } is { 'even' | if (isEven(it)) | otherwise('odd') }.\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
The digit five is odd.
The digit four is even.
The digit one is odd.
The digit three is odd.
The digit nine is odd.
The digit eight is even.
The digit six is even.
The digit seven is odd.
The digit two is even.
The digit zero is even.
".NormalizeNewLines()));
        }

        [Test]
        public void Linq11()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Product Info:
{{ products | select: { it.ProductName | raw } is in the category { it.Category } and costs { it.UnitPrice | currency } per unit.\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Product Info:
Chai is in the category Beverages and costs $18.00 per unit.
Chang is in the category Beverages and costs $19.00 per unit.
Aniseed Syrup is in the category Condiments and costs $10.00 per unit.
".NormalizeNewLines()));
        }

        [Test]
        public void Linq12()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Number: In-place?
{{ numbers | select: { it }: { it | equals(index) | lower }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Number: In-place?
5: false
4: false
1: false
3: true
9: false
8: false
6: true
7: true
2: false
0: false
".NormalizeNewLines()));
        }

        [Test]
        public void Linq13()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
Numbers < 5:
{{ numbers
   | where: it < 5 
   | select: { digits[it] }\n 
}}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Numbers < 5:
four
one
three
two
zero
".NormalizeNewLines()));
        }

        [Test]
        public void Linq14()
        {
            var context = CreateContext(new Dictionary<string, object>
            {
                {"numbersA", new[]{ 0, 2, 4, 5, 6, 8, 9 }},
                {"numbersB", new[]{ 1, 3, 5, 7, 8 }},
            });
            
            Assert.That(context.EvaluateTemplate(@"
Pairs where a < b:
{{ numbersA | zip(numbersB)
   | let({ a: 'it[0]', b: 'it[1]' })  
   | where: a < b 
   | select: { a } is less than { b }\n 
}}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Pairs where a < b:
0 is less than 1
0 is less than 3
0 is less than 5
0 is less than 7
0 is less than 8
2 is less than 3
2 is less than 5
2 is less than 7
2 is less than 8
4 is less than 5
4 is less than 7
4 is less than 8
5 is less than 7
5 is less than 8
6 is less than 7
6 is less than 8
".NormalizeNewLines()));
        }

        [Test]
        public void Linq15()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
{{ customers | zip: it.Orders
   | let({ c: 'it[0]', o: 'it[1]' })
   | where: o.Total < 500
   | select: ({ c.CustomerId }, { o.OrderId }, { o.Total | format('0.0#') })\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
(ALFKI, 10702, 330.0)
(ALFKI, 10952, 471.2)
(ANATR, 10308, 88.8)
(ANATR, 10625, 479.75)
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq16()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
{{ customers | zip: it.Orders
   | let({ c: 'it[0]', o: 'it[1]' })
   | where: o.OrderDate >= '1998-01-01' 
   | select: ({ c.CustomerId }, { o.OrderId }, { o.OrderDate })\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
(ALFKI, 10835, 1/15/1998 12:00:00 AM)
(ALFKI, 10952, 3/16/1998 12:00:00 AM)
(ALFKI, 11011, 4/9/1998 12:00:00 AM)
(ANATR, 10926, 3/4/1998 12:00:00 AM)
(ANTON, 10856, 1/28/1998 12:00:00 AM)
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq17()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
{{ customers | zip: it.Orders
   | let({ c: 'it[0]', o: 'it[1]' })
   | where: o.Total >= 2000 
   | select: ({ c.CustomerId }, { o.OrderId }, { o.Total | format('0.0#') })\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
(ANTON, 10573, 2082.0)
(AROUT, 10558, 2142.9)
(AROUT, 10953, 4441.25)
(BERGS, 10384, 2222.4)
(BERGS, 10524, 3192.65)
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq18()
        {
            var context = CreateContext();

            var template = @"
{{ '1997-01-01' | assignTo: cutoffDate }}
{{ customers 
   | where: it.Region = 'WA'
   | zip: it.Orders
   | let({ c: 'it[0]', o: 'it[1]' })
   | where: o.OrderDate  >= cutoffDate 
   | select: ({ c.CustomerId }, { o.OrderId })\n }}
";
            Assert.That(context.EvaluateTemplate(template.NormalizeNewLines()).NormalizeNewLines(),
                
                Does.StartWith(@"
(LAZYK, 10482)
(LAZYK, 10545)
(TRAIH, 10574)
(TRAIH, 10577)
(TRAIH, 10822)
(WHITC, 10469)
(WHITC, 10483)
(WHITC, 10504)
(WHITC, 10596)
(WHITC, 10693)
(WHITC, 10696)
(WHITC, 10723)
(WHITC, 10740)
(WHITC, 10861)
(WHITC, 10904)
(WHITC, 11032)
(WHITC, 11066)
".NormalizeNewLines()));
        }

        [Test]
        public void Linq19()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
{{ customers 
   | let({ cust: 'it', custIndex: 'index' })
   | zip: cust.Orders
   | let({ o: 'it[1]' })
   | select: Customer #{ custIndex | incr } has an order with OrderID { o.OrderId }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
Customer #1 has an order with OrderID 10643
Customer #1 has an order with OrderID 10692
Customer #1 has an order with OrderID 10702
Customer #1 has an order with OrderID 10835
Customer #1 has an order with OrderID 10952
Customer #1 has an order with OrderID 11011
Customer #2 has an order with OrderID 10308
Customer #2 has an order with OrderID 10625
Customer #2 has an order with OrderID 10759
Customer #2 has an order with OrderID 10926
".NormalizeNewLines()));
        }

        [Test]
        public void Linq20()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
First 3 numbers:
{{ numbers | take(3) | select: { it }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
First 3 numbers:
5
4
1
".NormalizeNewLines()));
        }

        [Test]
        public void Linq21()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
First 3 orders in WA:
{{ customers | zip: it.Orders 
   | let({ c: 'it[0]', o: 'it[1]' })
   | where: c.Region = 'WA'
   | select: { [c.CustomerId, o.OrderId, o.OrderDate] | jsv }\n 
}}
").NormalizeNewLines(),
                
                Does.StartWith(@"
First 3 orders in WA:
[LAZYK,10482,1997-03-21]
[LAZYK,10545,1997-05-22]
[TRAIH,10574,1997-06-19]
".NormalizeNewLines()));
        }

        [Test]
        public void Linq22()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
All but first 4 numbers:
{{ numbers | skip(4) | select: { it }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
All but first 4 numbers:
9
8
6
7
2
0
".NormalizeNewLines()));
        }

        [Test]
        public void Linq23()
        {
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
All but first 2 orders in WA:
{{ customers | zip: it.Orders
   | let({ c: 'it[0]', o: 'it[1]' })
   | where: c.Region = 'WA'
   | skip(2)
   | select: { [c.CustomerId, o.OrderId, o.OrderDate] | jsv }\n 
}}
").NormalizeNewLines(),
                
                Does.StartWith(@"
All but first 2 orders in WA:
[TRAIH,10574,1997-06-19]
[TRAIH,10577,1997-06-23]
[TRAIH,10822,1998-01-08]
[WHITC,10269,1996-07-31]
[WHITC,10344,1996-11-01]
[WHITC,10469,1997-03-10]
[WHITC,10483,1997-03-24]
[WHITC,10504,1997-04-11]
[WHITC,10596,1997-07-11]
[WHITC,10693,1997-10-06]
[WHITC,10696,1997-10-08]
[WHITC,10723,1997-10-30]
[WHITC,10740,1997-11-13]
[WHITC,10861,1998-01-30]
[WHITC,10904,1998-02-24]
[WHITC,11032,1998-04-17]
[WHITC,11066,1998-05-01]
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq24()
        { 
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
First numbers less than 6:
{{ numbers 
   | takeWhile: it < 6 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
First numbers less than 6:
5
4
1
3
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq25()
        { 
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
First numbers not less than their position:
{{ numbers 
   | takeWhile: it >= index 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
First numbers not less than their position:
5
4
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq26()
        { 
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
All elements starting from first element divisible by 3:
{{ numbers 
   | skipWhile: mod(it,3) != 0 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
All elements starting from first element divisible by 3:
3
9
8
6
7
2
0
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq27()
        { 
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
All elements starting from first element less than its position:
{{ numbers 
   | skipWhile: it >= index 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
All elements starting from first element less than its position:
1
3
9
8
6
7
2
0
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq28()
        { 
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
The sorted list of words:
{{ words 
   | orderBy: it 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
The sorted list of words:
apple
blueberry
cherry
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq29()
        { 
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
The sorted list of words (by length):
{{ words 
   | orderBy: it.Length 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
The sorted list of words (by length):
apple
cherry
blueberry
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq30()
        { 
            var context = CreateContext();

            Assert.That(context.EvaluateTemplate(@"
{{ products 
   | orderBy: it.ProductName 
   | select: { it | jsv }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
{ProductId:17,ProductName:Alice Mutton,Category:Meat/Poultry,UnitPrice:39,UnitsInStock:0}
{ProductId:3,ProductName:Aniseed Syrup,Category:Condiments,UnitPrice:10,UnitsInStock:13}
{ProductId:40,ProductName:Boston Crab Meat,Category:Seafood,UnitPrice:18.4,UnitsInStock:123}
{ProductId:60,ProductName:Camembert Pierrot,Category:Dairy Products,UnitPrice:34,UnitsInStock:19}
{ProductId:18,ProductName:Carnarvon Tigers,Category:Seafood,UnitPrice:62.5,UnitsInStock:42}
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq31()
        { 
            var context = CreateContext(new Dictionary<string, object>
            {
                { "words", new[] { "aPPLE", "AbAcUs", "bRaNcH", "BlUeBeRrY", "ClOvEr", "cHeRry" } },
                { "comparer", new CaseInsensitiveComparer() }
            });
            
            Assert.That(context.EvaluateTemplate(@"
{{ words 
   | orderBy('it.Length', { comparer }) 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
AbAcUs
aPPLE
BlUeBeRrY
bRaNcH
cHeRry
ClOvEr
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq32()
        { 
            var context = CreateContext();
            
            Assert.That(context.EvaluateTemplate(@"
The doubles from highest to lowest:
{{ doubles 
   | orderByDescending: it 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
The doubles from highest to lowest:
4.1
2.9
2.3
1.9
1.7
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq33()
        { 
            var context = CreateContext();

            Assert.That(context.EvaluateTemplate(@"
{{ products 
   | orderByDescending: it.UnitsInStock
   | select: { it | jsv }\n }}
").NormalizeNewLines(),
                
                Does.StartWith(@"
{ProductId:75,ProductName:Rhönbräu Klosterbier,Category:Beverages,UnitPrice:7.75,UnitsInStock:125}
{ProductId:40,ProductName:Boston Crab Meat,Category:Seafood,UnitPrice:18.4,UnitsInStock:123}
{ProductId:6,ProductName:Grandma's Boysenberry Spread,Category:Condiments,UnitPrice:25,UnitsInStock:120}
{ProductId:55,ProductName:Pâté chinois,Category:Meat/Poultry,UnitPrice:24,UnitsInStock:115}
{ProductId:61,ProductName:Sirop d'érable,Category:Condiments,UnitPrice:28.5,UnitsInStock:113}
".NormalizeNewLines()));
        }
        
        [Test]
        public void Linq34()
        { 
            var context = CreateContext(new Dictionary<string, object>
            {
                { "words", new[] { "aPPLE", "AbAcUs", "bRaNcH", "BlUeBeRrY", "ClOvEr", "cHeRry" } },
                { "comparer", new CaseInsensitiveComparer() }
            });
            
            Assert.That(context.EvaluateTemplate(@"
{{ words 
   | orderByDescending('it', { comparer }) 
   | select: { it }\n }}
").NormalizeNewLines(),
                
                Is.EqualTo(@"
ClOvEr
cHeRry
bRaNcH
BlUeBeRrY
aPPLE
AbAcUs
".NormalizeNewLines()));
        }
    }
}