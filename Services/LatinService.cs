using Models;
using System.Linq;
using Seido.Utilities.SeedGenerator;

namespace Services;


public class LatinService
{
    readonly List<LatinSentence> _latins = new SeedGenerator().LatinSentences(100).Select(l => new LatinSentence() {Sentence = l}).ToList();

    #region CRUD 
    public int Count(string filter)
    {
        filter ??= "";
        return _latins.Count(l =>
            l.Sentence.ToLower().Contains(filter.ToLower()));
    }

    public List<LatinSentence> ReadSentences(int? pageNumber, int? pageSize, string filter)
    {
        filter ??= "";
        if (pageNumber == null || pageSize == null)
        {
            return _latins.Where(l =>
                    l.Sentence.ToLower().Contains(filter.ToLower())).ToList();
        }

        return _latins.Where(l =>
                    l.Sentence.ToLower().Contains(filter.ToLower())).ToList()

            //Adding paging
            .Skip(pageNumber.Value * pageSize.Value)
            .Take(pageSize.Value).ToList();
    }

    public LatinSentence ReadSentence(Guid id)
    {
        var l = _latins.FirstOrDefault(l => l.SentenceId == id);
        if (l != null)
        {
            return new LatinSentence(l);
        }
        
        return null;
    }

    public LatinSentence UpdateQuote(LatinSentence _src)
    {
        var l = _latins.FirstOrDefault(l => l.SentenceId == _src.SentenceId);
        if (l != null)
        {
            l.Sentence = _src.Sentence;
            return new LatinSentence(l);
        }
        return null;
    }

    public LatinSentence CreateQuote(LatinSentence _src)
    {
       _latins.Add(_src); 
       return new LatinSentence(_src);
    }

    public LatinSentence DeleteQuote(Guid id)
    {
        var l = _latins.FirstOrDefault(l => l.SentenceId == id);

        //If the item does not exists
        if (l == null) throw new ArgumentException($"Item {id} is not existing");

        _latins.Remove(l);
        return l;
    }

    #endregion
}



