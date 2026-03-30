using System.Reflection;
using System.Runtime.ExceptionServices;
using Mews.Job.Scheduler.Core.EntityFrameworkCore.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;

namespace Mews.Job.Scheduler.UnitTests;

[TestFixture]
public class SqlActionExceptionMapperTests
{
    [Test]
    public void Map_ShouldReturnSameException_WhenOperationTimeoutExceptionIsPassed()
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new OperationTimeoutException());

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<OperationTimeoutException>());
    }

    [Test]
    public void Map_ShouldReturnSameException_WhenOperationCanceledExceptionIsPassed()
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new OperationCanceledException());

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void Map_ShouldReturnOperationTimeoutException_WhenTimeoutExceptionIsNested()
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", new TimeoutException()));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<OperationTimeoutException>());
    }

    [Test]
    public void Map_ShouldReturnConcurrencyException_WhenDbUpdateConcurrencyExceptionIsPassed()
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new DbUpdateConcurrencyException("", new List<IUpdateEntry>()));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<ConcurrencyException>());
    }

    [Test]
    [TestCase(2601)]
    [TestCase(2627)]
    public void Map_ShouldReturnUniquenessExceptionException_WhenSqlExceptionWithNumber2601Or2627IsPassed(int sqlExceptionNumber)
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", CreateSqlException("", sqlExceptionNumber)));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<UniquenessException>());
    }

    [Test]
    [TestCase(4002)]
    [TestCase(8009)]
    public void Map_ShouldReturnTransientPersistenceFaultException_WhenSqlExceptionWithNumber4002Or8009IsPassed(int sqlExceptionNumber)
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", CreateSqlException("", sqlExceptionNumber)));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<TransientPersistenceFaultException>());
    }

    [Test]
    public void Map_ShouldReturnTransientPersistenceFaultException_WhenWin32ExceptionIsPassed()
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", CreateSqlException(innerException: new System.ComponentModel.Win32Exception())));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<TransientPersistenceFaultException>());
    }

    [Test]
    public void Map_ShouldReturnTransientPersistenceFaultException_WhenSqlExceptionWithIoExceptionIsPassed()
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", CreateSqlException(innerException: new IOException("Unable to read data from the transport connection: Connection reset by peer."))));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<TransientPersistenceFaultException>());
    }

    [Test]
    public void Map_ShouldReturnTransientPersistenceFaultException_WhenSqlExceptionWithSqlExceptionSevereErrorMessageIsPassed()
    {
        var message = "A severe error occurred on the current command.  The results, if any, should be discarded.\r\nA severe error occurred on the current command.  The results, if any, should be discarded.";
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", CreateSqlException(message: message)));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);


        Assert.That(sut.SourceException, Is.InstanceOf<TransientPersistenceFaultException>());
    }

    [Test]
    public void Map_ShouldReturnTransientPersistenceFaultException_WhenSqlExceptionWithSqlExceptionSevereErrorOperationCanceledByUserMessageIsPassed()
    {
        var message = "A severe error occurred on the current command.  The results, if any, should be discarded.\r\nOperation cancelled by user.";
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", CreateSqlException(message: message)));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);


        Assert.That(sut.SourceException, Is.InstanceOf<TransientPersistenceFaultException>());
    }

    [Test]
    [TestCase("Conversion overflows.")]
    [TestCase("Parameter value '1' is out of range.")]
    public void Map_ShouldReturnInvalidValueException_WhenInnerInnerExceptionMessageStartsWithParameterValueOrSqlExceptionConversionOverflowsMessage(string message)
    {
        var dispatchInfo = ExceptionDispatchInfo.Capture(new Exception("", new Exception("", new Exception(message))));

        var sut = SqlActionExceptionMapper.Map(dispatchInfo);

        Assert.That(sut.SourceException, Is.InstanceOf<InvalidValueException>());
    }

    private SqlException CreateSqlException(string? message = default, int errorNumber = default, Exception? innerException = default)
    {
        var collection = Construct<SqlErrorCollection>(0);
        message ??= "This is a Mock-SqlException";
        var error = Construct<SqlError>(2, errorNumber, (byte)2, (byte)3, "server name", message, "proc", 100, new Exception());

        var errorCollectionAdd = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
        _ = errorCollectionAdd?.Invoke(collection, [error]);

        var exceptionCreate = typeof(SqlException).GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
            null, CallingConventions.ExplicitThis, [typeof(SqlErrorCollection), typeof(string), typeof(Guid), typeof(Exception)], []);
        return (exceptionCreate!.Invoke(null, new object[] { collection, "7.0.0", Guid.Empty, innerException! }) as SqlException)!;
    }

    private T Construct<T>(int ctorIndex, params object[] p)
    {
        var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
        var ctor = ctors[ctorIndex];
        return (T)ctor.Invoke(p);
    }
}
