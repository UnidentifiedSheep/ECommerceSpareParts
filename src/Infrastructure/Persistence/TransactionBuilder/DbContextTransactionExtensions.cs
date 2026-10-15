using System.Data;
using Abstractions.Interfaces;
using Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using Persistence.Interfaces;

namespace Persistence.TransactionBuilder;

public static class DbContextTransactionExtensions
{
	public static ICustomTransaction WithRetries(
		this DbContext context,
		int count,
		ITransactionExceptionClassifier exceptionClassifier) =>
		new TransactionConfig(context, exceptionClassifier).WithRetries(count);

	public static ICustomTransaction WithRetryDelay(
		this DbContext context,
		TimeSpan delay,
		ITransactionExceptionClassifier exceptionClassifier) =>
		new TransactionConfig(context, exceptionClassifier).WithRetryDelay(delay);

	public static ICustomTransaction WithIsolationLevel(
		this DbContext context,
		IsolationLevel isolationLevel,
		ITransactionExceptionClassifier exceptionClassifier) =>
		new TransactionConfig(context, exceptionClassifier).WithIsolationLevel(isolationLevel);

	public static ICustomTransaction WithDefaultTransactionSettings(
		this DbContext context,
		string variant,
		ITransactionExceptionClassifier exceptionClassifier) =>
		new TransactionConfig(context, exceptionClassifier).WithDefaultSettings(variant);
}
