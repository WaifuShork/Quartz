using System;
using System.Text;
using System.Threading;

namespace Vivian
{
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int _blockCount = 10;
        private readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0f / 8);
        private const string _animation = @"|/-\";

        private readonly Timer _timer;

        private double _currentProgress = 0;
        private string _currentText = string.Empty;

        private bool _disposed = false;
        private int _animationIndex = 0;

        public ProgressBar()
        {
            _timer = new Timer(TimerHandler!);

            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref _currentProgress, value);
        }

        private void TimerHandler(object state)
        {
            lock (_timer)
            {
                if (_disposed)
                {
                    return;
                }

                var progressBlockCount = (int) (_currentProgress * _blockCount);
                var percent = (int) (_currentProgress * 100);
                var text = string.Format("[{0}{1}] {2,3}% {3}", 
                                        new string('#', progressBlockCount), 
                                        new string('-', _blockCount - progressBlockCount), 
                                        percent, 
                                        _animation[_animationIndex++ % _animation.Length]);
                
                // Update 
                UpdateText(text);
                // Reset
                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            var commonPrefixLength = 0;
            var commonLength = Math.Min(_currentText.Length, text.Length);

            while (commonPrefixLength < commonLength &&
                   text[commonPrefixLength] == _currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            var outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', _currentText.Length - commonPrefixLength);

            outputBuilder.Append(text.Substring(commonPrefixLength));

            var overlapCount = _currentText.Length - text.Length;

            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }
        }

        private void ResetTimer()
        {
            _timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (_timer)
            {
                _disposed = true;
                UpdateText(string.Empty);
            }
        }
    }
}