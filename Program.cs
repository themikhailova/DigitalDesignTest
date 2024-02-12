using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        // проверка корректности запуска
        if (args.Length == 0)
        {
            Console.WriteLine("Запустите программу с указанием пути к файлу формата .fb2");
            return;
        }

        string filePath = args[0];

        // проверка существования файла
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файла не существует");
            return;
        }

        string fileExtension = Path.GetExtension(filePath).ToLower();

        // проверка расширения файла
        if (fileExtension != ".fb2" && fileExtension != ".txt") // сайт http://az.lib.ru/ предлагает скачивание файла в формате .fb2
        {
            Console.WriteLine("Файл должен иметь расширение .fb2 или .txt");
            return;
        }

        try
        {
            // чтение файла
            string text = File.ReadAllText(filePath);

            if (fileExtension == ".fb2")
            {
                // извлечение текста из тегов <p> и <title>
                MatchCollection matches = Regex.Matches(text, @"<p[^>]*>(.*?)</p>|<title[^>]*>(.*?)</title>", RegexOptions.Singleline);
                
                // объединение извлеченных фрагментов в одну строку
                text = string.Join(" ", matches.Cast<Match>().SelectMany(m => new[] {m.Groups[1].Value, m.Groups[2].Value})
                                                                            .Where(s => !string.IsNullOrEmpty(s)));
            }
            // символы-разделители
            char[] sep = {' ', '\n', '\r', '\t', '.', ',', ';', ':', '!', '?', '"', ']', '[', ')', '(', '<', '>', '/', '-', '='};

            // разделение текста на слова с удалением пустых элементов
            string[] words = text.Split(sep, StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !Regex.IsMatch(word, @"\d+")) // удаление слов с цифрами (например, "500-е")
                .ToArray();

            // словарь уникальных слов
            Dictionary<string, int> wordDict = new Dictionary<string, int>();

            // подсчет количества упоминаний каждого слова
            foreach (string word in words)
            {
                string lowRegWord = word.ToLower(); // перевод слова в нижний регистр для учета регистра
                if (wordDict.ContainsKey(lowRegWord)){wordDict[lowRegWord]++;} // если слово уже в словаре, то увеличить счетчик
                else {wordDict[lowRegWord] = 1;} // если слова нет в словаре - добавить с счетчиком 1
            }

            // сортировка
            var sortedWords = wordDict.OrderByDescending(pair => pair.Value);

            // путь для сохранения списка в формате .txt
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath); // удаление расширения
            string extension = ".txt"; 
            string newFileName = $"{fileNameWithoutExtension}_{DateTime.Now:yyyyMMddHHmmss}{extension}"; // добавление текущую дату и время к имени файла

            string output = Path.Combine(Path.GetDirectoryName(filePath) ?? "", newFileName);

            // запись
            using (StreamWriter writer = new StreamWriter(output))
            {
                writer.WriteLine($"Всего уникальных слов: {wordDict.Count}");
                foreach (var pair in sortedWords) {writer.WriteLine($"{pair.Key}: {pair.Value}");}            
            }

            Console.WriteLine($"Результаты сохранены в файл: {output}");
        }
        catch (Exception ex){Console.WriteLine($"Ошибка: {ex.Message}");}
    }
}
