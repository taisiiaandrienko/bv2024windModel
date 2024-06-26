namespace BV2024WindModel.Abstractions
{
    public interface ICalculator<TInput, out TOutput>
    {
        TOutput Calculate(in TInput input);
    }
}
