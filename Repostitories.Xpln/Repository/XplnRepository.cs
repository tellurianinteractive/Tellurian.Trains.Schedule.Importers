using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Tellurian.Trains.Models.Planning;
using Tellurian.Trains.Repositories.Interfaces;
using Tellurian.Trains.Repositories.Xpln.DataSetProviders;
using Tellurian.Trains.Repositories.Xpln.Extensions;

namespace Tellurian.Trains.Repositories.Xpln;
public sealed partial class XplnRepository : ILayoutReadStore, ITimetableReadStore, IScheduleReadStore, IDisposable
{
    public readonly IDataSetProvider DataSetProvider;
    private DataSet? DataSet;

    public XplnRepository(IDataSetProvider dataSetProvider)
    {
        DataSetProvider = dataSetProvider;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }


    public RepositoryResult<Layout> GetLayout(string fileName)
    {
        DataSet = GetData(fileName);
        var name = Path.GetFileNameWithoutExtension(fileName);
        var result = new Layout { Name = name };
        var messages = new List<Message>();
        var stations = AddStations(result, messages);
        if (stations.IsFailure) return stations;
        var routes = AddRoutes(result, messages);
        return routes;
    }

    private RepositoryResult<Layout> AddStations(Layout layout, List<Message> messages)
    {
        const string WorkSheetName = "StationTrack";
        const int Signature = 0;
        const int TrackName = 2;
        const int Lenght = 3;
        const int Name = 4;
        const int Type = 5;
        const int SubType = 6;
        const int Remark = 7;
        const int MinLength = 7;

        var stations = DataSet?.Tables[WorkSheetName];
        if (stations is null)
            return RepositoryResult<Layout>.Failure(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName));

        var rowNumber = 1;
        Station? current = null;
        foreach (DataRow station in stations.Rows)
        {
            if (rowNumber > 1)
            {
                var itemMessages = new List<Message>();
                var fields = station.GetRowFields();
                if (fields.IsEmptyFields()) { if (layout.Stations.Any()) break; else continue; }
                itemMessages.AddRange(ValidateRow(fields, rowNumber));
                if (itemMessages.HasNoStoppingErrors())
                {
                    if (fields[5].Is("Station"))
                    {
                        if (current is not null)
                        {
                            layout.Add(current);
                            current = null;
                        }
                        var validationMessages = ValidateStation(fields, rowNumber);
                        if (validationMessages.HasNoStoppingErrors())
                        {
                            current = CreateStation(fields);
                        }
                        itemMessages.AddRange(validationMessages);
                    }
                    else if (fields[5].Is("Track"))
                    {
                        if (current is null) continue;
                        var validationMessages = ValidateTrack(fields, rowNumber);
                        if (validationMessages.HasNoStoppingErrors())
                        {
                            current.Add(CreateTrack(fields));
                        }
                        itemMessages.AddRange(validationMessages);
                    }
                }
                messages.AddRange(itemMessages);
            }
            rowNumber++;
        }
        if (current is not null) layout.Add(current);

        if (messages.HasStoppingErrors())
            return RepositoryResult<Layout>.Failure(messages.ToStrings());
        else
            return RepositoryResult<Layout>.Success(layout, messages.ToStrings());

        static Station CreateStation(string[] fields) =>
            new()
            {
                Type = fields[Type],
                Name = fields[Name],
                Signature = fields[Signature],
                IsShadow = fields[SubType].Is("Depot")
            };

        static StationTrack CreateTrack(string[] fields) =>
            new(fields[TrackName])
            {
                IsMain = fields[SubType].Is("Main"),
                IsScheduled = fields[SubType].Is("Main", "Depot"),
                Usage = fields[Remark],
                DisplayOrder = fields[1].NumberOrZero(),
            };

        static Message[] ValidateRow(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields.Length < MinLength)
                messages.Add(Message.Error(Resources.Strings.NotAllFieldsArePresent, rowNumber, MinLength, fields.Length));
            if (!fields[Type].ValueOrEmpty().Is("Station", "Track"))
                messages.Add(Message.Error(Resources.Strings.UnsupportedType, rowNumber, fields[Type]));
            return messages.ToArray();
        }

        static Message[] ValidateStation(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Signature].IsEmpty())
                messages.Add(Message.Error(Resources.Strings.ColumnMustHaveAValue, rowNumber, "Name"));
            if (!fields[SubType].ValueOrEmpty().Is("Station", "Block"))
                messages.Add(Message.Error(Resources.Strings.UnsupportedSubType, rowNumber, fields[SubType]));
            return messages.ToArray();
        }

        static Message[] ValidateTrack(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[TrackName].IsEmpty())
                messages.Add(Message.Error(Resources.Strings.ColumnMustHaveAValue, rowNumber, "TrackName"));
            if (fields[Lenght].IsEmpty())
                messages.Add(Message.Warning(Resources.Strings.ColumnIsNotSpecified, rowNumber, "Length"));
            else if (!fields[Lenght].IsNumber())
                messages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, "Length", fields[Lenght]));
            if (!fields[SubType].ValueOrEmpty().Is("Main", "Siding", "Depot", "Goods"))
                messages.Add(Message.Error(Resources.Strings.UnsupportedSubType, rowNumber, fields[SubType]));
            return messages.ToArray();
        }
    }

    private RepositoryResult<Layout> AddRoutes(Layout layout, List<Message> messages)
    {
        const string WorkSheetName = "Routes";
        const int Route = 0;
        const int StartStation = 2;
        const int StartPosition = 3;
        const int EndStation = 4;
        const int EndPosition = 5;
        const int Speed = 6;
        const int Tracks = 7;
        const int Time = 8;

        var stations = DataSet?.Tables[WorkSheetName];
        if (stations is null)
            return RepositoryResult<Layout>.Failure(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName));
        var rowNumber = 1;
        foreach (DataRow station in stations.Rows)
        {
            if (rowNumber > 1)
            {
                var itemMessages = new List<Message>();
                var fields = station.GetRowFields();
                if (fields.IsEmptyFields()) { if (layout.Stations.Any()) break; else continue; }

                var start = layout.Station(fields[StartStation]);
                var end = layout.Station(fields[EndStation]);
                if (start.IsNone)
                    itemMessages.Add(Message.Error(Resources.Strings.StationNotFoundInLayout, rowNumber, fields[StartStation]));
                if (end.IsNone)
                    itemMessages.Add(Message.Error(Resources.Strings.StationNotFoundInLayout, rowNumber, fields[EndStation]));
                if (!fields[Tracks].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Tracks)));
                if (!fields[Speed].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Speed)));
                if (!fields[Time].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(Time)));
                if (!fields[EndPosition].IsNumber())
                    itemMessages.Add(Message.Error(Resources.Strings.ColumnMustBeANumber, rowNumber, nameof(EndPosition)));
                if (itemMessages.HasNoStoppingErrors())
                {
                    var routeNumber = fields[Route].AsInteger();
                    TimetableStretch? timetableStretch = null;
                    if (!layout.HasTimetableStretch(fields[Route]))
                    {
                        timetableStretch = new TimetableStretch(fields[Route]);
                        layout.Add(timetableStretch);
                    }
                    else
                    {
                        var ts = layout.TimetableStretch(fields[Route]);
                        if (ts.IsNone)
                        {
                            itemMessages.Add(Message.Error(Resources.Strings.RouteNotFoundInLayout, rowNumber, fields[Route]));
                        }
                        else
                        {
                            timetableStretch = ts.Value;
                        }
                    }
                    if (itemMessages.HasNoStoppingErrors())
                    {
                        var distance = Math.Abs(fields[EndPosition].AsDouble() - fields[StartPosition].AsDouble());
                        var stretch = new TrackStretch(start.Value, end.Value, distance, fields[Tracks].AsInteger(), fields[Speed].AsInteger(), fields[Time].AsInteger());
                        stretch = timetableStretch!.AddLast(stretch);
                        layout.Add(stretch);
                    }
                }
                messages.AddRange(itemMessages);
            }
            rowNumber++;
        }
        if (messages.HasStoppingErrors())
            return RepositoryResult<Layout>.Failure(messages.ToStrings());
        else
            return RepositoryResult<Layout>.Success(layout, messages.ToStrings());
    }

    public RepositoryResult<Timetable> GetTimetable(string fileName)
    {
        const string WorkSheetName = "Trains";
        const int Station = 2;
        const int Track = 3;
        const int Arrival = 4;
        const int Departure = 5;
        const int Object = 7;
        const int Type = 8;
        const int Remark = 10;
        const int MinLength = 10;

        var messages = new List<Message>();
        DataSet = GetData(fileName);
        var trains = DataSet?.Tables[WorkSheetName];
        if (trains is null)
        {
            messages.Add(Message.System(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName)));
            return RepositoryResult<Timetable>.Failure(messages.ToStrings());
        }

        var layout = GetLayout(fileName);
        var layoutMessages = layout.Messages.ToList();
        if (layout.IsFailure)
        {
            layoutMessages.Add(string.Format(CultureInfo.CurrentCulture, Resources.Strings.CannotReadTimetableDueToErrorsInLayout));
            return RepositoryResult<Timetable>.Failure(layoutMessages);
        }

        var result = new Timetable(fileName, layout.Item);
        var rowNumber = 1;
        var callNumber = 0;
        Train? current = null;

        foreach (DataRow row in trains.Rows)
        {
            if (rowNumber > 1)
            {
                var itemMessages = new List<Message>();
                var fields = row.GetRowFields();
                if (fields.IsEmptyFields()) { if (result.Trains.Any()) break; else continue; }
                itemMessages.AddRange(ValidateRow(fields, rowNumber));
                if (itemMessages.HasNoStoppingErrors())
                {
                    var type = fields[Type].ToLowerInvariant();
                    switch (type)
                    {
                        case "traindef":
                            {
                                if (current is not null)
                                {
                                    messages.AddRange(AddTrain(result, current, rowNumber));
                                    current = null;
                                    callNumber = 0;
                                }

                                var validationMessages = ValidateTrain(fields, rowNumber);
                                if (validationMessages.HasNoStoppingErrors())
                                {
                                    current = CreateTrain(fields);
                                }
                                messages.AddRange(validationMessages);
                            }
                            break;

                        case "timetable":
                            {
                                if (current is null) continue;
                                var validationMessages = ValidateCall(fields, rowNumber);
                                if (validationMessages.HasNoStoppingErrors())
                                {
                                    callNumber++;
                                    var track = layout.Item.Track(fields[Station], fields[Track]);
                                    if (track.IsNone)
                                    {
                                        messages.Add(Message.Error(track.Message));
                                    }
                                    else
                                    {
                                        current.Add(CreateCall(fields, track.Value));
                                    }
                                }
                                messages.AddRange(validationMessages);
                            }
                            break;

                        case "locomotive":
                            {
                                if (current is null) continue;
                                if (fields[Object].HasValue())
                                {
                                    var note = new Note()
                                    {
                                        IsDriverNote = true,
                                        IsStationNote = true,
                                        LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                                        Text = fields[Remark].HasValue() ?
                                        string.Format(CultureInfo.CurrentCulture, Resources.Strings.UseLocoClasses, fields[Object], fields[Remark]) :
                                            string.Format(CultureInfo.CurrentCulture, Resources.Strings.UseLoco, fields[Object])
                                    };
                                    var train = result.Trains.SingleOrDefault(t => t.Equals(current));
                                    if (train is not null) train.Calls.First().Notes.Add(note);
                                };
                            }
                            break;

                        case "trainset":
                            {
                                if (current is null) continue;
                                if (fields[Remark].HasValue())
                                {
                                    var note = new Note()
                                    {
                                        IsDriverNote = true,
                                        LanguageCode = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
                                        Text = fields[Remark]

                                    };
                                    current.Calls.First().Notes.Add(note);
                                };

                            }
                            break;
                    }
                }
                messages.AddRange(itemMessages);
            }
            rowNumber++;
        }
        if (current is not null) messages.AddRange(AddTrain(result, current, rowNumber));
        if (messages.HasStoppingErrors())
            return RepositoryResult<Timetable>.Failure(messages.ToStrings());
        else
            return RepositoryResult<Timetable>.Success(result, messages.ToStrings());

        static IEnumerable<Message> AddTrain(Timetable timetable, Train train, int rowNumber)
        {
            if (train.Calls.Count == 0)
            {
                return new[] { Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.TrainHasNoCalls, rowNumber, train)) };
            }
            else
            {
                timetable.Add(train.WithFixedSingleCallTrain().WithFixedFirstAndLastCall());
                return Enumerable.Empty<Message>();
            }

        }

        static Train CreateTrain(string[] fields) =>
            new(fields[Object].TrainNumber(), fields[Object])
            {
                Category = fields[Object].TrainCategory(),
            };

        static StationCall CreateCall(string[] fields, StationTrack track) =>
            new(track, fields[Arrival].AsTime(), fields[Departure].AsTime())
            {
                IsArrival = true,
                IsDeparture = true,

            };

        static Message[] ValidateRow(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields.Length < MinLength)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.NotAllFieldsArePresent, rowNumber, MinLength, fields.Length)));
            if (!fields[Arrival].IsTime())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Arrival", fields[Arrival])));
            if (!fields[Departure].IsTime())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Departure", fields[Arrival])));
            else if (!fields[Type].Is("Traindef", "Timetable", "Locomotive", "Trainset", "Job", "Wheel", "Group"))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnsupportedType, rowNumber, fields[Type])));
            return messages.ToArray();
        }

        static Message[] ValidateTrain(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object")));
            return messages.ToArray();
        }

        static Message[] ValidateCall(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Track].IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Track")));
            return messages.ToArray();
        }
    }

    public RepositoryResult<Schedule> GetSchedule(string filename)
    {
        const string WorkSheetName = "Trains";
        const int TrainNumber = 0;
        const int From = 2;
        const int To = 3;
        const int Arrival = 4;
        const int Departure = 5;
        const int Object = 7;
        const int Type = 8;
        const int TrainName = 9;
        const int MinLength = 9;

        var messages = new List<Message>();
        var locoSchedules = new Dictionary<string, LocoSchedule>(100);
        var trainsetSchedules = new Dictionary<string, TrainsetSchedule>(200);
        var driverDuties = new Dictionary<string, DriverDuty>();

        DataSet = GetData(filename);
        var trains = DataSet?.Tables[WorkSheetName];
        if (trains is null)
        {
            messages.Add(Message.System(string.Format(CultureInfo.CurrentCulture, Resources.Strings.WorksheetNotFound, WorkSheetName)));
            return RepositoryResult<Schedule>.Failure(messages.ToStrings());
        }
        var timetable = GetTimetable(filename);
        if (timetable.IsFailure)
        {
            return RepositoryResult<Schedule>.Failure(timetable.Messages);
        }
        var schedule = Schedule.Create(filename, timetable.Item);
        Train? currentTrain = null;

        var rowNumber = 1;
        foreach (DataRow row in trains.Rows)
        {
            if (rowNumber > 1)
            {
                var itemMessages = new List<Message>();
                var fields = row.GetRowFields();
                if (fields.IsEmptyFields()) { if (locoSchedules.Any() || trainsetSchedules.Any() || driverDuties.Any()) break; else continue; }
                itemMessages.AddRange(ValidateRow(fields, rowNumber));
                if (itemMessages.HasNoStoppingErrors())
                {
                    var type = fields[Type].ToLowerInvariant();
                    switch (type)
                    {
                        case "traindef":
                            {
                                var trainExternalId = fields[Object];
                                var train = schedule.Timetable.Train(trainExternalId);
                                if (train.IsNone)
                                {
                                    messages.Add(Message.Error(train.Message));
                                    currentTrain = null;
                                    break;
                                }
                                currentTrain = train.Value;
                            }
                            break;
                        case "locomotive":
                            {
                                if (currentTrain is null) break;

                                var locoMessages = new List<Message>();
                                locoMessages.AddRange(ValidateLoco(fields, rowNumber));

                                if (locoMessages.HasNoStoppingErrors())
                                {
                                    var locoId = fields[Object];
                                    if (!locoSchedules.ContainsKey(locoId))
                                        locoSchedules.Add(locoId, new LocoSchedule(locoId));
                                    if (locoSchedules.TryGetValue(locoId, out var loco))
                                    {
                                        var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                        locoMessages.AddRange(keys.Messages);
                                        if (locoMessages.HasNoStoppingErrors())
                                        {
                                            TrainPart trainPart = new TrainPart(keys.FromCall.Value, keys.ToCall.Value);
                                            loco.Add(trainPart);
                                        }
                                    }
                                }
                                messages.AddRange(locoMessages);
                            }
                            break;
                        case "trainset":
                            {
                                if (currentTrain is null) break;
                                var trainsetMessages = new List<Message>();
                                trainsetMessages.AddRange(ValidateTrainset(fields, rowNumber));
                                if (trainsetMessages.HasNoStoppingErrors())
                                {
                                    var trainsetId = fields[Object].OrElse(fields[TrainName]);
                                    if (!trainsetSchedules.ContainsKey(trainsetId))
                                        trainsetSchedules.Add(trainsetId, new TrainsetSchedule(trainsetId));
                                    if (trainsetSchedules.TryGetValue(trainsetId, out var trainset))
                                    {
                                        var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                        trainsetMessages.AddRange(keys.Messages);
                                        if (trainsetMessages.HasNoStoppingErrors())
                                        {
                                            TrainPart trainPart = new TrainPart(keys.FromCall.Value, keys.ToCall.Value);
                                            trainset.Add(trainPart);
                                        }
                                    }
                                }
                                messages.AddRange(trainsetMessages);
                            }
                            break;
                        case "job":
                            {
                                if (currentTrain is null) break;
                                var dutyMessages = new List<Message>();
                                dutyMessages.AddRange(ValidateJob(fields, rowNumber));
                                if (dutyMessages.HasNoStoppingErrors())
                                {
                                    var jobId = fields[Object].OrElse(fields[TrainNumber]);
                                    if (!driverDuties.ContainsKey(jobId))
                                        driverDuties.Add(jobId, new DriverDuty(jobId));
                                    if (driverDuties.TryGetValue(jobId, out var duty))
                                    {
                                        var keys = GetTrainPartKeys(fields, currentTrain, rowNumber);
                                        dutyMessages.AddRange(keys.Messages);
                                        if (dutyMessages.HasNoStoppingErrors())
                                        {
                                            TrainPart trainPart = new TrainPart(keys.FromCall.Value, keys.ToCall.Value);
                                            duty.Add(trainPart);
                                        }
                                    }

                                }
                                messages.AddRange(dutyMessages);
                            }
                            break;
                        case "wheel":
                            {
                                if (currentTrain is null) break;

                            }
                            break;
                        case "group":
                            {
                                if (currentTrain is null) break;

                            }
                            break;
                    }
                }

            }
            rowNumber++;
        }
        if (messages.HasStoppingErrors()) return RepositoryResult<Schedule>.Failure(messages.ToStrings());
        foreach (var loco in locoSchedules.Values) schedule.AddLocoSchedule(loco);
        foreach (var trainset in trainsetSchedules.Values) schedule.AddTrainsetSchedule(trainset);
        foreach (var duty in driverDuties.Values) schedule.AddDriverDuty(duty);
        return RepositoryResult<Schedule>.Success(schedule);

        static TrainPartKeys GetTrainPartKeys(string[] fields, Train currentTrain, int rowNumber)
        {
            var messages = new List<Message>();
            var (start, end, startTime, endTime) = GetTrainPartFields(fields);
            if (startTime > endTime)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectInTrainHasWrongTimingEndStartionIsBeforeStartStation, rowNumber, fields[Object], currentTrain, fields[Arrival].AsTime(), fields[Departure].AsTime())));
            var (fromCall, fromIndex) = currentTrain.FindBetweenArrivalAndDeparture(start, startTime, rowNumber);
            var (toCall, toIndex) = currentTrain.FindBetweenArrivalAndDeparture(end, endTime, rowNumber);
            if (messages.HasNoStoppingErrors())
            {
                if (fromCall.IsNone)
                {
                    messages.Add(Message.Error(fromCall.Message));
                    messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectAtStationWithDepartureDoNotRefersToAnExistingTimeInTrain, rowNumber, fields[Object], fields[From], fields[Departure].AsTime(), currentTrain)));
                }
                if (toCall.IsNone)
                {
                    messages.Add(Message.Error(toCall.Message));
                    messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectAtStationWithArrivalDoNotRefersToAnExistingTimeInTrain, rowNumber, fields[Object], fields[To], fields[Arrival].AsTime(), currentTrain)));
                }
                if (toCall.HasValue && fromCall.HasValue && fromIndex >= toIndex)
                    messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ObjectInTrainHasWrongTimingEndStartionIsBeforeStartStation, rowNumber, fields[Object], currentTrain, fields[Departure].AsTime(), fields[Arrival].AsTime())));
            }
            return new TrainPartKeys(fromCall, toCall, messages);
        }


        static (string from, string to, Time departure, Time arrival) GetTrainPartFields(string[] fields) =>
           (fields[From], fields[To], fields[Arrival].AsTime(), fields[Departure].AsTime());

        static Message[] ValidateRow(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields.Length < MinLength)
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.NotAllFieldsArePresent, rowNumber, MinLength, fields.Length)));
            if (!fields[Arrival].IsTime())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Arrival", fields[Arrival])));
            if (!fields[Departure].IsTime())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustBeATime, rowNumber, "Departure", fields[Arrival])));
            else if (!fields[Type].Is("Traindef", "Timetable", "Locomotive", "Trainset", "Job", "Wheel", "Group"))
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.UnsupportedType, rowNumber, fields[Type])));
            return messages.ToArray();
        }

        static Message[] ValidateLoco(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object")));
            return messages.ToArray();
        }
        static Message[] ValidateJob(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].OrElse(fields[TrainNumber]).IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object|TrainNumber")));
            return messages.ToArray();
        }
        static Message[] ValidateTrainset(string[] fields, int rowNumber)
        {
            var messages = new List<Message>();
            if (fields[Object].IsEmpty() && fields[TrainName].IsEmpty())
                messages.Add(Message.Error(string.Format(CultureInfo.CurrentCulture, Resources.Strings.ColumnMustHaveAValue, rowNumber, "Object|TrainName")));
            return messages.ToArray();
        }
    }

    private DataSet GetData(string filename)
    {
        if (DataSet is not null) return DataSet;
        var data = DataSetProvider.LoadFromFile(filename);
        if (data is null) throw new FileNotFoundException(filename);
        return data;
    }

    #region IDisposable

    private bool IsDisposed;
    private void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                DataSet?.Dispose();
                if (DataSetProvider is IDisposable disposable) disposable.Dispose();
            }
            IsDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
