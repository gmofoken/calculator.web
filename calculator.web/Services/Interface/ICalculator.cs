namespace calculator.web.Services.Interface
{
    public interface ICalculator
    {
        public double Parse(string expression, bool isRadians = true);
    }
}
