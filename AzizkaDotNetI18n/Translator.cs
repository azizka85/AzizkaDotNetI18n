using AzizkaDotNetI18n.Options;

namespace AzizkaDotNetI18n;

public class Translator
{
    protected DataOptions? data;
    protected Dictionary<string, string> globalContext = new Dictionary<string, string>();

    protected Func<string, int?, Dictionary<string, string>?, Dictionary<string, object>, string>? extension;

    public void Add(DataOptions data)
    {
        if (this.data == null)
        {
            this.data = data;
        }
        else
        {
            foreach (var item in data.Values)
            {
                this.data.Values[item.Key] = item.Value;
            }

            if (data.Contexts != null && data.Contexts.Count > 0)
            {
                if (this.data.Contexts == null)
                {
                    this.data.Contexts = new List<ContextOptions>();
                }
                
                this.data.Contexts.AddRange(data.Contexts);
            }
        }
    }

    public void SetContext(string key, string value)
    {
        globalContext[key] = value;
    }

    public void ClearContext(string key)
    {
        globalContext.Remove(key);
    }

    public void Extend(Func<string, int?, Dictionary<string, string>?, Dictionary<string, object>, string> extension)
    {
        this.extension = extension;
    }

    public void ResetData()
    {
        data = null;
    }

    public void ResetContext()
    {
        globalContext = new Dictionary<string, string>();
    }

    public void Reset()
    {
        ResetData();
        ResetContext();
    }

    public string Translate(string text, params object[] input)
    {
        int? num = null;
        Dictionary<string, string>? formatting = null;
        object? defaultNumOrFormatting = null;
        object? numOrFormattingOrContext = null;
        object? formattingOrContext = null;

        if (input.Length > 0)
        {
            defaultNumOrFormatting = input[0];
        }

        if (input.Length > 1)
        {
            numOrFormattingOrContext = input[1];
        }

        if (input.Length > 2)
        {
            formattingOrContext = input[2];
        }

        var context = globalContext;

        if (defaultNumOrFormatting is Dictionary<string, string>)
        {
            formatting = (Dictionary<string, string>)defaultNumOrFormatting;

            if (numOrFormattingOrContext is Dictionary<string, string>)
            {
                context = (Dictionary<string, string>)numOrFormattingOrContext;
            }
        }
        else if (defaultNumOrFormatting is int)
        {
            num = (int)defaultNumOrFormatting;

            if (numOrFormattingOrContext is Dictionary<string, string>)
            {
                formatting = (Dictionary<string, string>)numOrFormattingOrContext;
            }

            if (formattingOrContext is Dictionary<string, string>)
            {
                context = (Dictionary<string, string>)formattingOrContext;
            }
        }
        else
        {
            if (numOrFormattingOrContext is int)
            {
                num = (int)numOrFormattingOrContext;

                if (formattingOrContext is Dictionary<string, string>)
                {
                    formatting = (Dictionary<string, string>)formattingOrContext;
                }
            }
            else
            {
                if (numOrFormattingOrContext is Dictionary<string, string>)
                {
                    formatting = (Dictionary<string, string>)numOrFormattingOrContext;
                }

                if (formattingOrContext is Dictionary<string, string>)
                {
                    context = (Dictionary<string, string>)formattingOrContext;
                }
            }
        }

        return TranslateText(text, num, formatting, context);
    }

    public string TranslateText(
        string text, 
        int? num, 
        Dictionary<string, string>? formatting,
        Dictionary<string, string> context
    ) {
        if (data == null)
        {
            return UseOriginalText(text, num, formatting);
        }

        var contextData = GetContextData(data, context);

        string result = "";
        var ok = false;
        
        if(contextData != null)
        {
            (result, ok) = FindTranslation(text, num, formatting, contextData.Values);
        }

        if(!ok)
        {
            (result, ok) = FindTranslation(text, num, formatting, data.Values);
        }

        if(!ok)
        {
            result = UseOriginalText(text, num, formatting);
        }

        return result;
    }

    public (string result, bool ok) FindTranslation(
        string text,
        int? num,
        Dictionary<string, string>? formatting,
        Dictionary<string, object> data
    ) {
        if (data.ContainsKey(text))
        {
            var value = data[text];

            if (value is Dictionary<string, object>)
            {
                if (extension != null)
                {
                    var result = extension(text, num, formatting, (Dictionary<string, object>)value);

                    result = ApplyNumbers(result, num);

                    return (ApplyFormatting(result, formatting), true);
                }
                else
                {
                    return (UseOriginalText(text, num, formatting), true);
                }
            }

            if (num == null)
            {
                if (value is string)
                {
                    return (ApplyFormatting((string)value, formatting), true);
                }
            }
            else if (value is List<List<object>>)
            {
                foreach (var triple in (List<List<object>>)value)
                {
                    int? low = triple.Count > 0 && triple[0] is int ? (int)triple[0] : null;
                    int? high = triple.Count > 1 && triple[1] is int ? (int)triple[1] : null;
                    string val = triple.Count > 2 && triple[2] is string ? (string)triple[2] : "";

                    if (
                        num == null && low == null && high == null ||
                        num != null && ((
                            low != null && num >= low && (high == null || num <= high) ||
                            low == null && high != null && num <= high
                        ))
                    )
                    {
                        var result = ApplyNumbers(val, num);

                        return (ApplyFormatting(result, formatting), true);
                    }
                }
            }
        }

        return (result: "", ok: false);
    }

    public string ApplyNumbers(string text, int? num)
    {
        if (num != null)
        {
            text = text.Replace("-%n", "" + (-num));
            text = text.Replace("%n", "" + num);
        }

        return text;
    }

    public string ApplyFormatting(string text, Dictionary<string, string>? formatting)
    {
        if (formatting != null)
        {
            foreach (var item in formatting)
            {
                var tpl = "%{" + item.Key + "}";
                text = text.Replace(tpl, item.Value);
            }
        }

        return text;
    }

    public ContextOptions? GetContextData(DataOptions data, Dictionary<string, string> context)
    {
        if (data.Contexts == null)
        {
            return null;
        }

        foreach (var ctx in data.Contexts)
        {
            var equal = true;

            foreach (var item in ctx.Matches)
            {
                equal = equal && context.ContainsKey(item.Key) && item.Value == context[item.Key];
                
                if(!equal) break;
            }

            if (equal) return ctx;
        }

        return null;
    }

    public string UseOriginalText(string text, int? num, Dictionary<string, string>? formatting)
    {
        if (num == null)
        {
            return ApplyFormatting(text, formatting);
        }

        return ApplyFormatting(text.Replace("%n", "" + num), formatting);
    }
}