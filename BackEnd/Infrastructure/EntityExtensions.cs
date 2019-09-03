﻿using BackEnd.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace BackEnd.Infrastructure
{
    public static class EntityExtensions
    {
        public static ConferenceDTO.SessionResponse MapSessionResponse(this Session session) =>
            new ConferenceDTO.SessionResponse
            {
                ID = session.ID,
                Title = session.Title,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Tags = session.SessionTags?
                              .Select(st => new ConferenceDTO.Tag
                              {
                                  ID = st.TagId,
                                  Name = st.Tag.Name
                              })
                               .ToList(),
                Speakers = session.SessionSpeakers?
                                  .Select(ss => new ConferenceDTO.Speaker
                                  {
                                      ID = ss.SpeakerId,
                                      Name = ss.Speaker.Name
                                  })
                                   .ToList(),
                TrackId = session.TrackId ?? 0,
                Track = new ConferenceDTO.Track
                {
                    ID = session.TrackId ?? 0,
                    Name = session.Track?.Name
                },
                ConferenceId = session.ConferenceId,
                Abstract = session.Abstract
            };

        public static ConferenceDTO.SpeakerResponse MapSpeakerResponse(this Speaker speaker) =>
            new ConferenceDTO.SpeakerResponse
            {
                ID = speaker.ID,
                Name = speaker.Name,
                Bio = speaker.Bio,
                WebSite = speaker.WebSite,
                Sessions = speaker.SessionSpeakers?
                    .Select(ss =>
                        new ConferenceDTO.Session
                        {
                            ID = ss.SessionId,
                            Title = ss.Session.Title
                        })
                    .ToList()
            };

        public static ConferenceDTO.AttendeeResponse MapAttendeeResponse(this Attendee attendee) =>
            new ConferenceDTO.AttendeeResponse
            {
                ID = attendee.ID,
                FirstName = attendee.FirstName,
                LastName = attendee.LastName,
                UserName = attendee.UserName,
                Sessions = attendee.SessionAttendees?
                    .Select(sa =>
                        new ConferenceDTO.Session
                        {
                            ID = sa.Session.ID,
                            Title = sa.Session.Title,
                            StartTime = sa.Session.StartTime,
                            EndTime = sa.Session.EndTime
                        })
                    .ToList(),
                Conferences = attendee.ConferenceAttendees?
                    .Select(ca =>
                        new ConferenceDTO.Conference
                        {
                            ID = ca.Conference.ID,
                            Name = ca.Conference.Name
                        })
                    .ToList(),
                Images = attendee.Images?
                    .Select(i =>
                        new ConferenceDTO.Image
                        {
                            ID = i.ID,
                            AttendeeId = i.AttendeeId,
                            UploadDate = i.UploadDate,
                            Content = i.Content,
                            Name = i.Name,
                        })
                    .ToList(),
            };

        public static ConferenceDTO.ImageResponse MapImageResponse(this Image image) =>
            new ConferenceDTO.ImageResponse
            {
                Attendee = new ConferenceDTO.Attendee
                {
                    ID = image.Attendee.ID,
                    EmailAddress = image.Attendee.EmailAddress,
                    FirstName = image.Attendee.FirstName,
                    LastName = image.Attendee.LastName,
                    UserName = image.Attendee.UserName,
                },
                AttendeeId = image.AttendeeId,
                Content = image.Content,
                ID = image.ID,
                Name = image.Name,
                UploadDate = image.UploadDate,
                ImageType = image.ImageType
            };

        public static byte[] ToByteArray<T>(this T obj)
        {
            if (obj == null)
            {
                return null;
            }

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);

                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(this byte[] data)
        {
            if (data == null)
            {
                return default(T);
            }

            var bf = new BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                return (T) bf.Deserialize(ms);
            }
        }
    }
}
