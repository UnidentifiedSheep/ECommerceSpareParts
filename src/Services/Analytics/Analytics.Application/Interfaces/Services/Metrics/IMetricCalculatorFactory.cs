namespace Analytics.Application.Interfaces.Services.Metrics;

public interface IMetricCalculatorFactory
{
    IMetricCalculator GetCalculator(Type metricType);
}