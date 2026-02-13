// Copyright (c) 2025-2026 Otto
// Лицензия: MIT (см. LICENSE)

using CredentialManagement;
using QRCoder;
using System;
using System.IO;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using tik4net;

namespace WiFi_Окошко
{
    internal partial class FormWiFi : Form
    {
        // Имя записи в диспетчере учётных данных Windows (Credential Manager) для логина с портом (формат "User:222") и пароля
        const string CredentialTarget = "WiFi_Window";

        // Фиксированные имена виртуального интерфейса и профиля безопасности в Mikrotik
        private const string VirtualInterfaceName = "Client_WLAN";
        private const string VirtualSecurityProfileName = "security_Client_WLAN";

        // Таймер автозавершения (обновляет заголовок окна)
        private Timer autoCloseTimer;

        // Интервал автозавершения в миллисекундах (по умолчанию 1 минута)
        private const int AutoCloseIntervalMs = 60_000;

        // Оставшееся время в секундах (используется для отображения обратного отсчёта)
        private int remainingSeconds;
        
        // Базовый заголовок формы (значение "this.Text" из дизайнера)
        private readonly string baseTitle;

        // Минимальная и максимальная ширина для SSID/Passwd
        private const int MinLabelWidth = 150;
        private const int MaxLabelWidth = 300;

        // Максимальная ширина формы
        private const int MaxFormWidth = 517;

        // Отступ между Label и PictureBox
        private const int Gap = 10;

        internal FormWiFi()
        {
            InitializeComponent();
           // this.Load += FormWiFi_Load;

            // Сохраняет первоначальный заголовк
            baseTitle = string.IsNullOrWhiteSpace(this.Text) ? "WiFi Окошко" : this.Text.Trim();
        }

        // Имя файла конфигурации (создаётся рядом с исполняемым файлом)
        private const string ConfigFileName = "WiFi_Window.conf";

        // Путь к файлу конфигурации (рядом с исполняемым файлом)
        private static string GetConfigFilePath()
        {
            string dir = Path.GetDirectoryName(Application.ExecutablePath);
            return Path.Combine(dir, ConfigFileName);
        }

        // Создаёт файл конфигурации с указанным шлюзом (только если файл ещё не существует, перезаписывать не нужно)
        private static void CreateConfigIfNotExists(string gateway)
        {
            string path = GetConfigFilePath();
            if (File.Exists(path)) return;

            var sb = new StringBuilder();
            sb.AppendLine("# Использовать кастомный шлюз?");
            sb.AppendLine("# Если \"false\" - будет игнорироваться указанный шлюз в данном конфиге и осуществляться каждый раз поиск шлюза по умолчанию и подключение к нему.");
            sb.AppendLine("# Если \"true\" - будет использоваться шлюз указанный ниже.");
            sb.AppendLine("Use_a_custom_gateway=false");
            sb.AppendLine();
            sb.AppendLine("# Шлюз");
            sb.AppendLine($"gateway={gateway}");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        // Читает значение ключа из конфигурационного файла (формат "ключ=значение", строки с '#' — комментарии)
        private static string ReadConfigValue(string key)
        {
            string path = GetConfigFilePath();
            if (!File.Exists(path)) return null;

            foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                int eqIndex = trimmed.IndexOf('=');
                if (eqIndex < 0) continue;

                string k = trimmed.Substring(0, eqIndex).Trim();
                string v = trimmed.Substring(eqIndex + 1).Trim();

                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                    return v;
            }
            return null;
        }

        // Главный обработчик загрузки: ищет шлюз, загружает креды, извлекает порт, запускает подключение и обновляет UI
        private async void FormWiFi_Load(object sender, EventArgs e)
        {
            // Показывает начальное состояние только в поле SSID
            SetLabelText(SSID, "Ждём...");

            // Запускает таймер автозавершения (начинает отсчёт с запуска формы)
            StartAutoCloseTimer();

            // Определяет шлюз: из конфига (если Use_a_custom_gateway=true) или автоматически
            string gatewayHost = null;
            bool configExists = File.Exists(GetConfigFilePath());

            if (configExists)
            {
                // Конфиг существует — проверяет флаг Use_a_custom_gateway
                string useCustom = ReadConfigValue("Use_a_custom_gateway");
                if (string.Equals(useCustom, "true", StringComparison.OrdinalIgnoreCase))
                {
                    // Использует шлюз из конфига
                    string cfgGateway = ReadConfigValue("gateway");
                    if (!string.IsNullOrWhiteSpace(cfgGateway) && IPAddress.TryParse(cfgGateway.Trim(), out _))
                    {
                        gatewayHost = cfgGateway.Trim();
                    }
                    else
                    {
                        // Шлюз в конфиге указан неверно — показывает ошибку и не пытается искать шлюз по умолчанию
                        SetLabelText(SSID, "Ошибка: неверный шлюз в конфиге");
                        SetControlFontBold(SSID, false);
                        SetLabelText(Passwd, string.Empty);
                        SetControlFontBold(Passwd, false);
                        return;
                    }
                }
            }

            // Если шлюз ещё не определён (конфига нет или Use_a_custom_gateway=false) — ищет шлюз по умолчанию
            if (gatewayHost == null)
            {
                var gw = GetDefaultGatewayIPv4();
                if (gw == null)
                {
                    SetLabelText(SSID, "Шлюз не найден");   // Показывает ошибку в SSID
                    SetControlFontBold(SSID, false);        // Делает обычный шрифт
                    SetLabelText(Passwd, string.Empty);     // Очищает пароль
                    SetControlFontBold(Passwd, false);      // Делает обычный шрифт
                    return;
                }
                gatewayHost = gw.ToString();

                // Создаём конфиг при первом запуске (если файла ещё нет — записывает найденный шлюз)
                if (!configExists)
                {
                    CreateConfigIfNotExists(gatewayHost);
                }
            }

            // Загружает учётные данные из диспетчера учётных данных Windows
            if (!TryLoadCredentials(CredentialTarget, out var apiUser, out var apiPass))
            {
                SetLabelText(SSID, "Учётные данные не найдены");    // Показывает ошибку в SSID
                SetControlFontBold(SSID, false);                    // Делает обычный шрифт
                SetLabelText(Passwd, string.Empty);                 // Очищает пароль
                SetControlFontBold(Passwd, false);                  // Делает обычный шрифт
                return;
            }

            // Порт берётся из имени пользователя (формат "User:222")
            int apiPort = -1;

            // Извлекает порт из имени пользователя
            if (!string.IsNullOrEmpty(apiUser))
            {
                var split = apiUser.Split([':'], StringSplitOptions.RemoveEmptyEntries);
                if (split.Length >= 2)
                {
                    var lastPart = split[split.Length - 1];
                    if (int.TryParse(lastPart, out var p2) && p2 > 0 && p2 <= 65535)
                    {
                        apiPort = p2;

                        // Восстанавливает имя пользователя без порта
                        char sep = ':';
                        apiUser = string.Join(sep.ToString(), split, 0, split.Length - 1);
                    }
                }
            }

            // Порт не найден
            if (apiPort == -1)
            {
                SetLabelText(SSID, "Порт не задан");    // Показывает ошибку в SSID
                SetControlFontBold(SSID, false);        // Делает обычный шрифт
                SetLabelText(Passwd, string.Empty);     // Очищает пароль
                SetControlFontBold(Passwd, false);      // Делает обычный шрифт
                return;
            }

            try
            {
                // Выполняет подключение асинхронно (в Task.Run, чтобы не блокировать UI)
                var tuple = await Task.Run(() => ConnectAndFetchFromMikrotik(gatewayHost, apiPort, apiUser, apiPass)); var ssid = tuple.ssid;
                var passwd = tuple.passwd;
                var profileBlock = tuple.profileBlock;

                if (!string.IsNullOrEmpty(ssid))
                {
                    // Успешно получили SSID — показывает и ставим жирный шрифт
                    SetLabelText(SSID, ssid);
                    SetControlFontBold(SSID, true);
                }
                else
                {
                    // Нет SSID — показывает сообщение об ошибке в SSID и делает обычный шрифт
                    SetLabelText(SSID, "SSID не найден");
                    SetControlFontBold(SSID, false);
                }

                if (!string.IsNullOrEmpty(passwd))
                {
                    // Успешно получили пароль — показывает и ставим жирный шрифт
                    SetLabelText(Passwd, passwd);
                    SetControlFontBold(Passwd, true);

                    ShowQrCode(ssid, passwd, profileBlock); // Показывает QR в "PictureBoxQr"

                    // Подгоняет размеры и ширину формы
                    AdjustControlsWidthAndForm();

                }
                else
                {
                    // Пароля нет — очищает поле и делает обычный шрифт
                    SetLabelText(Passwd, string.Empty);
                    SetControlFontBold(Passwd, false);
                }
            }
            catch (Exception ex)
            {
                // При исключении: показывает ошибку в SSID (не жирным), пароль очищает
                SetLabelText(SSID, "Ошибка: " + ex.Message);
                SetControlFontBold(SSID, false);

                SetLabelText(Passwd, string.Empty);
                SetControlFontBold(Passwd, false);
            }
        }

        // Вспомогательный метод: измеряет ширину текста внутри RichTextBox + запас (pad)
        private int MeasureTextWidthWithPadding(RichTextBox rtb, int paddingPx)
        {
            if (rtb == null) return MinLabelWidth;
            using Graphics g = rtb.CreateGraphics();

            // MeasureString иногда добавляет небольшой хвост; Ceiling округляет в большую сторону
            SizeF size = g.MeasureString(rtb.Text ?? string.Empty, rtb.Font);
            return (int)Math.Ceiling(size.Width) + paddingPx;
        }

        // Обновляет ширину RichTextBox, позицию QR и ширину формы
        private void AdjustControlsWidthAndForm()
        {
            if (SSID == null || Passwd == null || PictureBoxQr == null) return;

            // Измеряет требуемую ширину для каждого поля (без применения к форме)
            int widthSsid = MeasureTextWidthWithPadding(SSID, 5);
            int widthPass = MeasureTextWidthWithPadding(Passwd, 5);

            // Ограничивает по Min/Max
            widthSsid = Math.Min(MaxLabelWidth, Math.Max(MinLabelWidth, widthSsid));
            widthPass = Math.Min(MaxLabelWidth, Math.Max(MinLabelWidth, widthPass));

            // Устанавливает ширины (не двигает Left'ы)
            SSID.Width = widthSsid;
            Passwd.Width = widthPass;

            // Правый край каждого поля
            int rightSsid = SSID.Left + SSID.Width;
            int rightPass = Passwd.Left + Passwd.Width;

            // Берём максимальную правую границу (чтобы QR оказался правее наиболее широкего поля)
            int rightMost = Math.Max(rightSsid, rightPass);

            // Новая позиция QR — справа от самого правого поля + промежуток Gap
            int newQrLeft = rightMost + Gap;

            // Рассчитает необходимую ширину формы (с запасом справа)
            int neededFormWidth = newQrLeft + PictureBoxQr.Width + 15;

            // Если нужно, расширяет форму, но не больше MaxFormWidth
            int newFormWidth = Math.Min(MaxFormWidth, Math.Max(this.Width, neededFormWidth));
            this.Width = newFormWidth;

            // Если форма очень узкая (например ограничено MaxFormWidth) — не даём QR уйти за правую границу формы
            int maxAllowedQrLeft = Math.Max(0, this.Width - PictureBoxQr.Width - 15);
            if (newQrLeft > maxAllowedQrLeft)
                newQrLeft = maxAllowedQrLeft;

            // Двигает PictureBox
            PictureBoxQr.Left = newQrLeft;
        }

        // Метод подгоняет ширину RichTextBox по тексту и сдвигает QR, увеличивая форму по необходимости
        private void AdjustRichTextBoxWidthAndForm(RichTextBox rtb, PictureBox qr)
        {
            if (rtb == null || qr == null) return;

            // Если вызван не из UI-потока — перенаправим выполнение в UI-поток
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AdjustRichTextBoxWidthAndForm(rtb, qr)));
                return;
            }

            // Гарантирует, что PictureBox не "приклеен" к правому краю или докнут
            qr.Dock = DockStyle.None;
            qr.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            const int rightPadding = 15; // Отступ от правого края PictureBox до края формы
            this.SuspendLayout();

            try
            {
                using Graphics g = rtb.CreateGraphics();
                // Измеряет ширину текста с текущим шрифтом
                SizeF textSize = g.MeasureString(rtb.Text, rtb.Font);

                // Ширина (с запасом)
                int desiredWidth = (int)Math.Ceiling(textSize.Width) + 5;
                desiredWidth = Math.Max(MinLabelWidth, Math.Min(MaxLabelWidth, desiredWidth));

                // Максимально возможная ширина RichTextBox при текущем MaxFormWidth
                int maxAllowedWidth = MaxFormWidth - (qr.Width + Gap + rtb.Left + rightPadding);
                if (maxAllowedWidth < MinLabelWidth) maxAllowedWidth = MinLabelWidth;

                if (desiredWidth <= maxAllowedWidth)
                {
                    // Ширина помещается без превышения MaxFormWidth
                    rtb.Width = desiredWidth;

                    int neededFormWidth = rtb.Left + rtb.Width + Gap + qr.Width + rightPadding;

                    // Увеличивает форму, если нужно (но не превышает MaxFormWidth)
                    if (neededFormWidth > this.Width)
                        this.Width = Math.Min(MaxFormWidth, neededFormWidth);
                }
                else
                {
                    // Ширина НЕ помещается в рамках MaxFormWidth, попробует расширить форму до нужного размера (если возможно)
                    int neededFormWidth = rtb.Left + desiredWidth + Gap + qr.Width + rightPadding;
                    if (neededFormWidth <= MaxFormWidth)
                    {
                        // Может расширить форму и поставить desiredWidth
                        this.Width = Math.Max(this.Width, neededFormWidth);
                        rtb.Width = desiredWidth;
                    }
                    else
                    {
                        // Нельзя расширить до нужного — ограничивает rtb так, чтобы QR оставался видимым
                        rtb.Width = Math.Max(MinLabelWidth, maxAllowedWidth);

                        // И ставим форму в максимум
                        this.Width = MaxFormWidth;
                    }
                }

                // После изменения ширины rtb — корректирует позицию QR
                qr.Left = rtb.Left + rtb.Width + Gap;

                // Коррекция: не позволяет qr уйти за правую границу client area
                int clientRight = this.ClientSize.Width;
                if (qr.Left + qr.Width + rightPadding > clientRight)
                {
                    int newLeft = Math.Max(rtb.Left + rtb.Width + Gap, clientRight - qr.Width - rightPadding);
                    qr.Left = newLeft;
                }

                // Принудительная перерисовка
                qr.Refresh();
                rtb.Refresh();
                this.Refresh();
            }
            finally
            {
                this.ResumeLayout();
            }
        }
        

        // Генерирует QR-код из SSID и пароля и отображает его в PictureBoxQr
        private void ShowQrCode(string ssid, string password, string chosenProfileBlock)
        {
            if (string.IsNullOrEmpty(ssid)) return;

            string wifiType;

            if (string.IsNullOrEmpty(password))
            {
                wifiType = "nopass";
            }
            else if (chosenProfileBlock != null)
            {
                bool hasWpa2 = Regex.IsMatch(chosenProfileBlock, @"\bwpa2-pre-shared-key=""[^""]+""", RegexOptions.IgnoreCase);
                bool hasWpa = Regex.IsMatch(chosenProfileBlock, @"\bwpa-pre-shared-key=""[^""]+""", RegexOptions.IgnoreCase);

                if (hasWpa2) wifiType = "WPA2";
                else if (hasWpa) wifiType = "WPA";
                else wifiType = "WPA"; // Пароль есть, но ключей явно нет — безопаснее WPA
            }
            else
            {
                wifiType = "WPA"; // Запасной вариант
            }

            // Формирует текст для QR-кода
            string qrText = $"WIFI:T:{wifiType};S:{ssid};P:{password};";

            using var qrGen = new QRCodeGenerator();
            var qrData = qrGen.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            using var qr = new QRCode(qrData);

            // Генерирует исходный QR, 1 модуль = 1 пиксель
            Bitmap qrBmp = qr.GetGraphic(1, Color.Black, Color.White, true);

            int targetWidth = PictureBoxQr.Width;
            int targetHeight = PictureBoxQr.Height;

            // Вычисляет масштаб, чтобы QR вписался в PictureBox
            float scaleX = (float)targetWidth / qrBmp.Width;
            float scaleY = (float)targetHeight / qrBmp.Height;
            float scale = Math.Min(scaleX, scaleY); // Cохраняет пропорции

            int newWidth = (int)(qrBmp.Width * scale);
            int newHeight = (int)(qrBmp.Height * scale);

            // Центрирует QR в PictureBox
            Bitmap finalBmp = new(targetWidth, targetHeight);
            using (Graphics g = Graphics.FromImage(finalBmp))
            {
                g.Clear(Color.White);
                int x = (targetWidth - newWidth) / 2;
                int y = (targetHeight - newHeight) / 2;
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(qrBmp, x, y, newWidth, newHeight);
            }

            // Помещает в контрол
            PictureBoxQr.Image = finalBmp;
        }

        // Попытка загрузить имя пользователя и пароль из указанного Target в Credential Manager (возвращает true, если успешно прочитаны)
        private static bool TryLoadCredentials(string target, out string username, out string password)
        {
            username = null;
            password = null;

            try
            {
                using var cred = new Credential { Target = target, Type = CredentialType.Generic };
                if (!cred.Load())
                    return false;

                username = cred.Username;
                password = cred.Password;
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Запускает таймер с интервалом 1 сек для обновления заголовка и автозакрытия
        private void StartAutoCloseTimer()
        {
            // Если уже есть — остановим и удалим
            if (autoCloseTimer != null)
            {
                autoCloseTimer.Stop();
                autoCloseTimer.Tick -= AutoCloseTimer_Tick;
                autoCloseTimer.Dispose();
                autoCloseTimer = null;
            }

            remainingSeconds = AutoCloseIntervalMs / 1000;

            autoCloseTimer = new Timer
            {
                Interval = 1000 // Обновляет каждую секунду
            };
            autoCloseTimer.Tick += AutoCloseTimer_Tick;
            autoCloseTimer.Start();

            // Сразу обновим заголовок
            UpdateWindowTitle(remainingSeconds);
        }

        // Обновляет обратный отсчёт в заголовке и корректно завершает приложение при нуле
        private void AutoCloseTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds = Math.Max(0, remainingSeconds - 1);

            // Обновляет заголовок
            UpdateWindowTitle(remainingSeconds);

            if (remainingSeconds <= 0)
            {
                // Остановим таймер и корректно завершим приложение
                if (autoCloseTimer != null)
                {
                    autoCloseTimer.Stop();
                    autoCloseTimer.Tick -= AutoCloseTimer_Tick;
                    autoCloseTimer.Dispose();
                    autoCloseTimer = null;
                }

                Application.Exit();
            }
        }

        // Обновляет "this.Text" в формате: "{baseTitle} | Завершение через: NN сек."
        private void UpdateWindowTitle(int secondsLeft)
        {
            try
            {
                this.Text = $"{baseTitle} | (Завершение через: {secondsLeft} сек.)";
            }
            catch
            {
                // В редком случае защиты от потоков — игнорирует
            }
        }

        // Универсальный установщик текста: если контрол RichTextBox - делает цветной рендер, иначе обычный текст
        private void SetLabelText(Control ctrl, string text)
        {
            if (ctrl == null) return;

            // Если вызов из другого потока — использует Invoke
            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(new Action(() => SetLabelText(ctrl, text)));
                return;
            }

            if (ctrl is RichTextBox rtb)
            {
                // Для RichTextBox использует посимвольный рендер
                RenderColoredText(rtb, text ?? string.Empty);
            }
            else if (ctrl is Label lbl)
            {
                lbl.Text = text ?? string.Empty;
            }
            else
            {
                ctrl.Text = text ?? string.Empty;
            }
        }

        // Устанавливает жирность шрифта (true = Bold, false = Regular)
        private void SetControlFontBold(Control ctrl, bool bold)
        {
            if (ctrl == null) return;

            if (ctrl.InvokeRequired)
            {
                ctrl.Invoke(new Action(() => SetControlFontBold(ctrl, bold)));
                return;
            }

            var currentFont = ctrl.Font ?? SystemFonts.DefaultFont;
            FontStyle style = bold ? FontStyle.Bold : FontStyle.Regular;
            // Сохраняет размер и семейство, заменяет только стиль
            ctrl.Font = new Font(currentFont.FontFamily, currentFont.Size, style);
        }

        // Рендерим текст по-символьно: буквы - чёрные, цифры - синие, спецсимволы - розовые
        private void RenderColoredText(RichTextBox rtb, string text)
        {
            if (rtb == null) return;

            rtb.SuspendLayout();
            rtb.ReadOnly = false;
            rtb.Clear();

            // Настройка стиля рендера
            var colorLetter = Color.Black;
            var colorDigit = Color.Blue;
            var colorSpecial = Color.DeepPink;

            foreach (var ch in text)
            {
                if (char.IsLetter(ch))
                {
                    rtb.SelectionColor = colorLetter;
                }
                else if (char.IsDigit(ch))
                {
                    rtb.SelectionColor = colorDigit;
                }
                else
                {
                    // Пробелы оставим нейтральными (черными)
                    if (char.IsWhiteSpace(ch))
                        rtb.SelectionColor = colorLetter;
                    else
                        rtb.SelectionColor = colorSpecial;
                }

                rtb.AppendText(ch.ToString());
            }

            // Сбрасывает выделение и ставим в начало
            rtb.SelectionStart = 0;
            rtb.SelectionLength = 0;
            rtb.ReadOnly = true;
            rtb.ResumeLayout();
        }

        // Логика подключения через RouterOS API и чтения данных WiFi
        private (string ssid, string passwd, string profileBlock) ConnectAndFetchFromMikrotik(string host, int port, string user, string pass)
        {
            using var connection = ConnectionFactory.CreateConnection(TikConnectionType.Api);
            connection.Open(host, port, user, pass);

            try
            {
                string ssid = "";
                string wpa = "";
                string wpa2 = "";

                // Получает SSID с виртуального интерфейса
                // Формат: /interface/wireless/print где name=Client_WLAN
                var ssidCmd = connection.CreateCommand("/interface/wireless/print");
                var ssidResult = ssidCmd.ExecuteList();

                foreach (var sentence in ssidResult)
                {
                    // Ищет интерфейс по имени
                    if (sentence.Words.TryGetValue("name", out string name) &&
                        name == VirtualInterfaceName)
                    {
                        sentence.Words.TryGetValue("ssid", out ssid);
                        ssid ??= "";
                        break;
                    }
                }

                // Получает ключи из профиля безопасности
                var profileCmd = connection.CreateCommand("/interface/wireless/security-profiles/print");
                var profileResult = profileCmd.ExecuteList();

                foreach (var sentence in profileResult)
                {
                    // Ищет профиль по имени
                    if (sentence.Words.TryGetValue("name", out string name) &&
                        name == VirtualSecurityProfileName)
                    {
                        sentence.Words.TryGetValue("wpa-pre-shared-key", out wpa);
                        sentence.Words.TryGetValue("wpa2-pre-shared-key", out wpa2);
                        wpa ??= "";
                        wpa2 ??= "";
                        break;
                    }
                }

                // Декодирует escape-последовательности если есть
                if (!string.IsNullOrEmpty(ssid))
                    ssid = DecodeMikrotikEscapes(ssid.Trim());
                if (!string.IsNullOrEmpty(wpa2))
                    wpa2 = DecodeMikrotikEscapes(wpa2.Trim());
                if (!string.IsNullOrEmpty(wpa))
                    wpa = DecodeMikrotikEscapes(wpa.Trim());

                // Приоритет пароля: WPA2 -> WPA
                string passwd = !string.IsNullOrEmpty(wpa2) ? wpa2 : (!string.IsNullOrEmpty(wpa) ? wpa : null);

                // Собирает псевдо-блок профиля для ShowQrCode
                var sb = new StringBuilder();
                sb.Append($"name=\"{VirtualSecurityProfileName}\"");
                if (!string.IsNullOrEmpty(wpa2)) sb.Append($" wpa2-pre-shared-key=\"{wpa2}\"");
                if (!string.IsNullOrEmpty(wpa)) sb.Append($" wpa-pre-shared-key=\"{wpa}\"");
                string profileBlock = sb.ToString();

                return (ssid, passwd, profileBlock);
            }
            finally
            {
                connection.Close();
            }
        }

        // Декодирует подряд идущие escape-последовательности вида "\HH\HH\HH" в текст
        // Если последовательность успешно даёт кириллический текст при декодировании Windows-1251 — используется он, иначе пробует UTF-8 и другие fallback'ы.
        // Не затрагивает обычный ASCII/латинский текст.
        private string DecodeMikrotikEscapes(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Найдём подряд идущие группы "\HH\HH..."
            return Regex.Replace(input, @"(\\[0-9A-Fa-f]{2})+", match =>
            {
                var token = match.Value; // Например "\D2\E5\F1\F2" (Тест)
                var hexMatches = Regex.Matches(token, @"\\([0-9A-Fa-f]{2})");
                var bytes = new byte[hexMatches.Count];
                for (int i = 0; i < hexMatches.Count; i++)
                {
                    bytes[i] = Convert.ToByte(hexMatches[i].Groups[1].Value, 16);
                }

                // Использует разные кодировки в порядке приоритетов:
                // 1) Windows-1251 (часто Mikrotik/Win-encoded Cyrillic)
                // 2) UTF-8
                // 3) ISO-8859-1 (как безопасный fallback)
                // 4) System.Text.Encoding.Default
                string TryWith(Encoding enc)
                {
                    try
                    {
                        return enc.GetString(bytes);
                    }
                    catch
                    {
                        return null;
                    }
                }

                var candidates = new (Encoding enc, string name)[]
                {
                    (Encoding.GetEncoding(1251), "cp1251"),
                    (Encoding.UTF8, "utf8"),
                    (Encoding.GetEncoding("ISO-8859-1"), "iso-8859-1"),
                    (Encoding.Default, "default")
                };

                // Сначала ищет декодирование, которое даёт кириллицу
                foreach (var (enc, name) in candidates)
                {
                    var s = TryWith(enc);
                    if (!string.IsNullOrEmpty(s) && Regex.IsMatch(s, @"\p{IsCyrillic}"))
                        return s;
                }

                // Если кириллицы не оказалось, попробует UTF-8 без символа замены
                var utf8 = TryWith(Encoding.UTF8);
                if (!string.IsNullOrEmpty(utf8) && !utf8.Contains("\uFFFD"))
                    return utf8;

                // Иначе пробует CP1251
                var cp1251 = TryWith(Encoding.GetEncoding(1251));
                if (!string.IsNullOrEmpty(cp1251))
                    return cp1251;

                // В крайнем случае — вернём исходный token (чтобы не терять данные)
                return token;
            });
        }

        // Находит IPv4-адрес шлюза по умолчанию среди активных сетевых интерфейсов для подключения к Mikrotik'у
        private IPAddress GetDefaultGatewayIPv4()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;

                var ipProps = ni.GetIPProperties();
                if (ipProps == null) continue;

                foreach (var ga in ipProps.GatewayAddresses)
                {
                    if (ga?.Address == null) continue;
                    if (ga.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        // Отбрасывает 127.0.0.1 и 0.0.0.0, берём реальный адрес шлюза
                        if (!IPAddress.IsLoopback(ga.Address) && !ga.Address.Equals(IPAddress.Parse("0.0.0.0")))
                        return ga.Address;
                    }
                }
            }
            return null;
        }

        // Ссылка на страницу автора
        private void LinkLabelAuthor_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://gitflic.ru/project/otto/wifi_window");
        }
    }
}
