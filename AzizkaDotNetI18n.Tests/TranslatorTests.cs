using AzizkaDotNetI18n.Options;
using Newtonsoft.Json.Linq;

namespace AzizkaDotNetI18n.Tests
{
    public class TranslatorTests
    {
        [Fact]
        public void TestTranslateHello()
        {
            var key = "Hello";
            var value = "Hello translated";

            var translator = Translator.Create(
                new DataOptions
                {
                    Values = new Dictionary<string, object>
                    {
                        {key, value}
                    }
                }
            );

            Assert.Equal(value, translator.Translate(key));
        }

        [Fact]
        public void TestTranslatePluralText()
        {
            var key = "%n comments";


            var zeroComments = "0 comments";
            var oneComment = "1 comment";
            var twoComments = "2 comments";
            var tenComments = "10 comments";

            var translator = Translator.Create(
                new DataOptions
                {
                    Values = new Dictionary<string, object>
                    {
                        {
                            key,
                            new List<List<object>>
                            {
                                new List<object> { 0, 0, "%n comments" },
                                new List<object> { 1, 1, "%n comment" },
                                new List<object> { 2, null, "%n comments" }
                            }
                        }
                    }
                }
            );            

            Assert.Equal(zeroComments, translator.Translate(key, 0));
            Assert.Equal(oneComment, translator.Translate(key, 1));
            Assert.Equal(twoComments, translator.Translate(key, 2));
            Assert.Equal(tenComments, translator.Translate(key, 10));
        }

        [Fact]
        public void TestTranslatePluralTextWithNegativeNumber()
        {
            var key = "Due in %n days";

            var dueTenDaysAgo = "Due 10 days ago";
            var dueTwoDaysAgo = "Due 2 days ago";
            var dueYesterday = "Due Yesterday";
            var dueToday = "Due Today";
            var dueTomorrow = "Due Tomorrow";
            var dueInTwoDays = "Due in 2 days";
            var dueInTenDays = "Due in 10 days";

            var translator = Translator.Create(
                new DataOptions
                {
                    Values = new Dictionary<string, object>
                    {
                        { key, new List<List<object>> {
                            new List<object> { null, -2, "Due -%n days ago" },
                            new List<object> { -1, -1, "Due Yesterday" },
                            new List<object> { 0, 0, "Due Today" },
                            new List<object> { 1, 1, "Due Tomorrow" },
                            new List<object> { 2, null, "Due in %n days" }
                        }}
                    }
                }
            );

            Assert.Equal(dueTenDaysAgo, translator.Translate(key, -10));
            Assert.Equal(dueTwoDaysAgo, translator.Translate(key, -2));
            Assert.Equal(dueYesterday, translator.Translate(key, -1));
            Assert.Equal(dueToday, translator.Translate(key, 0));
            Assert.Equal(dueTomorrow, translator.Translate(key, 1));
            Assert.Equal(dueInTwoDays, translator.Translate(key, 2));
            Assert.Equal(dueInTenDays, translator.Translate(key, 10));
        }

        [Fact]
        public void TestTranslateTextWithFormatting()
        {
            var key = "Welcome %{name}";
            var value = "Welcome John";

            var translator = new Translator();

            Assert.Equal(
                value, 
                translator.Translate(
                    key,
                    new Dictionary<string, string>
                    {
                        { "name", "John" }
                    }
                )
            );            
        }

        [Fact]
        public void TestTranslateTextUsingContexts()
        {
            var key = "%{name} updated their profile";

            var johnValue = "John updated his profile";
            var janeValue = "Jane updated her profile";                        

            var translator = Translator.Create(
                new DataOptions
                {
                    Contexts = new List<ContextOptions>
                    {
                        new ContextOptions
                        {
                            Matches = new Dictionary<string, string>
                            {
                                { "gender", "male" }
                            },
                            Values = new Dictionary<string, object>
                            {
                                { key, "%{name} updated his profile" }
                            }
                        },
                        new ContextOptions
                        {
                            Matches = new Dictionary<string, string>
                            {
                                { "gender", "female" }
                            },
                            Values = new Dictionary<string, object>
                            {
                                { key, "%{name} updated her profile" }
                            }
                        }
                    }
                }
            );

            Assert.Equal
            (
                johnValue,
                translator.Translate
                (
                    key,
                    new Dictionary<string, string>
                    {
                        { "name", "John" }
                    },
                    new Dictionary<string, string>
                    {
                        { "gender", "male" }
                    }
                )
            );

            Assert.Equal
            (
                janeValue,
                translator.Translate
                (
                    key,
                    new Dictionary<string, string>
                    {
                        { "name", "Jane" }
                    },
                    new Dictionary<string, string>
                    {
                        { "gender", "female" }
                    }
                )
            );
        }

        [Fact]
        public void TestTranslatePluralTextUsingContexts()
        {
            var key = "%{name} uploaded %n photos to their %{album} album";

            var johnValue = "John uploaded 1 photo to his Buck's Night album";
            var janeValue = "Jane uploaded 4 photos to her Hen's Night album";

            var translator = Translator.Create(
                new DataOptions
                {
                    Contexts = new List<ContextOptions>
                    {
                        new ContextOptions
                        {
                            Matches = new Dictionary<string, string>
                            {
                                { "gender", "male" }
                            },
                            Values = new Dictionary<string, object>
                            {
                                {
                                    key,
                                    new List<List<object>>
                                    {
                                        new List<object> {0, 0, "%{name} uploaded %n photos to his %{album} album"},
                                        new List<object> {1, 1, "%{name} uploaded %n photo to his %{album} album"},
                                        new List<object> {2, null, "%{name} uploaded %n photos to his %{album} album"}
                                    }
                                }
                            }
                        },
                        new ContextOptions
                        {
                            Matches = new Dictionary<string, string>
                            {
                                { "gender", "female" }
                            },
                            Values = new Dictionary<string, object>
                            {
                                {
                                    key,
                                    new List<List<object>>
                                    {
                                        new List<object> {0, 0, "%{name} uploaded %n photos to her %{album} album"},
                                        new List<object> {1, 1, "%{name} uploaded %n photo to her %{album} album"},
                                        new List<object> {2, null, "%{name} uploaded %n photos to her %{album} album"}
                                    }
                                }
                            }
                        }
                    }
                }
            );

            Assert.Equal
            (
                johnValue,
                translator.Translate
                (
                    key,
                    1,
                    new Dictionary<string, string>
                    {
                        { "name", "John" },
                        { "album", "Buck's Night" }
                    },
                    new Dictionary<string, string>
                    {
                        { "gender", "male" }
                    }
                )
            );

            Assert.Equal
            (
                janeValue,
                translator.Translate
                (
                    key,
                    4,
                    new Dictionary<string, string>
                    {
                        { "name", "Jane" },
                        { "album", "Hen's Night" }
                    },
                    new Dictionary<string, string>
                    {
                        { "gender", "female" }
                    }
                )
            );
        }

        [Fact]
        public void TestTranslatePluralTextUsingExtension()
        {
            var key = "%n results";

            var zeroResults = "нет результатов";
            var oneResult = "1 результат";
            var elevenResults = "11 результатов";
            var fourResults = "4 результата";
            var results = "101 результат";

            var translator = Translator.Create(
                new DataOptions
                {
                    Values = new Dictionary<string, object>
                    {
                        { key, new Dictionary<string, object> {
                            { "zero", "нет результатов" },
                            { "one", "%n результат" },
                            { "few", "%n результата" },
                            { "many", "%n результатов" },
                            { "other", "%n результаты" }
                        }}
                    }
                }
            );
            
            var getPluralisationKey = (int? num) =>
            {
                if (num == null || num == 0)
                {
                    return "zero";
                }

                if (num % 10 == 1 && num % 100 == 11)
                {
                    return "one";
                }

                if (
                    (num % 10 == 2 || num % 10 == 3 || num % 10 == 4) &&
                    num % 100 != 12 && num % 100 != 13 && num % 100 != 14
                ) {
                    return "few";
                }
                
                if (num % 10 == 0 || num % 10 == 5 || num % 10 == 6 || num % 10 == 7 || num % 10 == 8 || num % 10 == 9 ||
                    num % 100 == 11 || num % 100 == 12 || num % 100 == 13 || num %100 == 14
                ) {
                    return "many";
                }

                return "other";
            };

            var russianExtension = (
                string text, 
                int? num, 
                Dictionary<string, string>? formatting,
                Dictionary<string, object> data
            ) => {
                var key = getPluralisationKey(num);
                
                return data.ContainsKey(key) ? data[key].ToString() : "";
            };
            
            translator.Extend(russianExtension);
            
            Assert.Equal
            (
                zeroResults,
                translator.Translate(key, 0)
            );
        }
    }
}