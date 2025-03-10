﻿using ASP.Data.Entities;
using ASP.Models.Content.Room;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace ASP.Data.DAL
{
    public class ContentDao
    {
        private readonly Object _dblocker;
        private readonly DataContext _context;
        public ContentDao(DataContext context, object dblocker)
        {
            _context = context;
            _dblocker = dblocker;
        }
        public void AddCategory(String name, String description, String? photoUrl, String? slug = null)
        {
            lock (_dblocker)
            {
                _context.Categories.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    DeleteDt = null,
                    PhotoUrl = photoUrl,
                    Slug = slug ?? name
                });
                _context.SaveChanges();
            }
        }
        public List<Category> GetCategories(bool includedDeleted = false)
        {
            List<Category> list;
            lock (_dblocker)
            {
                list = _context
                    .Categories
                    .Where(c => includedDeleted || c.DeleteDt == null)
                    .ToList();
            }
            return list;
        }
        public Category? GetCategoryBySlug(String slug)
        {
            Category? ctg;
            lock (_dblocker)
            {
                ctg = _context.Categories.FirstOrDefault(c => c.Slug == slug);
            }
            return ctg;
        }
        public Category? GetCategoryById(Guid id)
        {
            Category? ctg;
            lock (_dblocker)
            {
                ctg = _context.Categories.Find(id);
            }
            return ctg;
        }
        public void UpdateCategory(Category category)
        {
            Category? ctg;
            lock (_dblocker)
            {
                ctg = _context.Categories.Find(category.Id);
            }
            if (ctg != null || ctg == category)
            {
                ctg.Name = category.Name;
                ctg.Description = category.Description;
                ctg.DeleteDt = category.DeleteDt;
                ctg.PhotoUrl = category.PhotoUrl;
            }
            lock (_dblocker)
            {
                _context.SaveChanges();
            }
        }
        public void DeleteCategory(Guid id)
        {
            var ctg = _context
                .Categories
                .Find(id);
            if (ctg != null || ctg.DeleteDt == null)
            {
                ctg.DeleteDt = DateTime.Now;
                lock (_dblocker)
                {
                    _context.SaveChanges();
                }
            }
        }
        public void RestoreCategory(Guid id)
        {
            var ctg = _context
                .Categories
                .Find(id);
            if (ctg != null || ctg.DeleteDt != null)
            {
                ctg.DeleteDt = null;
                lock (_dblocker)
                {
                    _context.SaveChanges();
                }
            }
        }

        public void DeleteCategory(Category category)
        {
            DeleteCategory(category.Id);
        }
        public void AddLocation(String name, String description, Guid CategoryId,
            int? Stars = null, Guid? CountryId = null,
            Guid? CityId = null, string? Address = null, String? PhotoUrl = null, String? slug = null)
        {
            lock (_dblocker)
            {
                _context.Locations.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    CategoryId = CategoryId,
                    Stars = Stars,
                    CountryId = CountryId,
                    CityId = CityId,
                    Address = Address,
                    DeleteDt = null,
                    PhotoUrl = PhotoUrl,
                    Slug = slug ?? name
                });
                _context.SaveChanges();
            }
        }

        public List<Location> GetLocations(String categorySlug, bool includedDeleted)
        {
            var ctg = GetCategoryBySlug(categorySlug);
            if (ctg == null)
            {
                return new List<Location>();
            }
            var query = _context
                .Locations
                .Where(loc => (includedDeleted || loc.DeleteDt == null) && loc.CategoryId == ctg.Id);
            return query.ToList();
        }
        public Location? GetLocationBySlug(String slug)
        {
            Location? ctg;
            lock (_dblocker)
            {
                ctg = _context.Locations.FirstOrDefault(c => c.Slug == slug);
            }
            return ctg;
        }
        public Location? GetLocationById(Guid? id)
        {

            Location? loc;
            lock (_dblocker)
            {
                loc = _context.Locations.FirstOrDefault(l => l.Id == id);
            }
            return loc;
        }
		public Room? GetRoomById(String id)
		{
			Room? room = _context.Rooms.FirstOrDefault(r => r.Id == Guid.Parse(id));
			return room == null ? null : room;
		}
		public void UpdateLocation(Location location)
        {
            Location? loc;
            lock (_dblocker)
            {
                loc = _context.Locations.Find(location.Id);
            }
            if (loc != null || loc == location)
            {
                loc.Name = location.Name;
                loc.Description = location.Description;
                loc.DeleteDt = location.DeleteDt;
                loc.PhotoUrl = location.PhotoUrl;
            }
            lock (_dblocker)
            {
                _context.SaveChanges();
            }
        }

        public void DeleteLocation(Guid id)
        {
            var loc = _context
                .Locations
                .Find(id);
            if (loc != null || loc.DeleteDt == null)
            {
                loc.DeleteDt = DateTime.Now;
                lock (_dblocker)
                {
                    _context.SaveChanges();
                }
            }
        }
        public void DeleteLocation(Location loc)
        {
            DeleteCategory(loc.Id);
        }

        public void RestoreLocation(Guid id)
        {
            var loc = _context
                .Locations
                .Find(id);
            if (loc != null || loc.DeleteDt != null)
            {
                loc.DeleteDt = null;
                lock (_dblocker)
                {
                    _context.SaveChanges();
                }
            }
        }

        public void AddRoom(String name, String description,
            String photoUrl, String slug, Guid locationId, int stars,
            Double dailyPrice)
        {
            lock (_dblocker)
            {
                _context.Rooms.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Description = description,
                    PhotoUrl = photoUrl,
                    Slug = slug,
                    LocationId = locationId,
                    Stars = stars,
                    DailyPrice = dailyPrice
                });
                _context.SaveChanges();
            }
        }
        public List<Room> GetRooms(String locationSlug)
        {
            Location? location;
            lock (_dblocker)
            {
                location = _context.Locations.
                FirstOrDefault(loc => loc.Slug == locationSlug);
            }
            if (location == null)
            {
                throw new Exception("Slug belong to NO location");
            }
            return GetRooms(location.Id);
        }
        public List<Room> GetRooms(Guid locationId)
        {
            List<Room> res;
            lock (_dblocker)
            {
                res = [.. _context.Rooms.Where(r => r.LocationId == locationId).ToList()];
            }
            return res;
        }

        public Room? GetRoomBySlug(String slug)
        {
            Guid? id;
            try { id = Guid.Parse(slug); }
            catch { id = null; }
            var slugSelector = (Room c) => c.Slug == slug;
            var idSelector = (Room c) => c.Id == id;

            Room? room;
            lock (_dblocker)
            {
                room = _context.Rooms
                    .Include(r => r.Reservations)
                    .FirstOrDefault(id == null ? slugSelector : idSelector);
            }
            if (room != null)
            {
                room.Reservations =
                    room.Reservations.Where(r => r.DeleteDt == null).ToList();
            }
            return room;
        }

        public void ReserveRoom(ReserveRoomFormModel model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            if (model.Date < DateTime.Today)
            {
                throw new ArgumentException("Date must not be in past");
            }
            Room? room;
            lock (_dblocker)
            {
                room = _context.Rooms.Find(model.RoomId);
            }
            if (room == null)
            {
                throw new ArgumentException("Room not found for id: " + model.RoomId);
            }
            lock (_dblocker)
            {
                _context.Reservations.Add(new()
                {
                    Id = Guid.NewGuid(),
                    Date = model.Date,
                    RoomId = model.RoomId,
                    UserId = model.UserId,
                    Price = room.DailyPrice,
                    OrderDt = DateTime.Now
                });
                _context.SaveChanges();
            }
        }

        public Reservation? GetReservation(Guid roomId, DateTime date)
        {
            Reservation? reservation;
            lock (_dblocker)
            {
                reservation = _context.Reservations
                    .FirstOrDefault(r => 
                    r.RoomId == roomId && 
                    r.Date.Date == date.Date &&
                    r.DeleteDt == null);
            }
            return reservation;
        }

        public void DeleteReservation(Guid id)
        {
            Reservation? reservation;
            lock (_dblocker)
            {
                reservation = _context.Reservations.Find(id);
            }
            if (reservation == null)
            {
                throw new ArgumentException("Passed id not found");
            }
            if (reservation.DeleteDt != null)
            {
                throw new ArgumentException("Passed id already deleted");
            }
            reservation.DeleteDt = DateTime.Now;
            lock (_dblocker)
            {
                _context.SaveChanges();
            }
        }
        public List<Location> GetLocationsByName(String Name)
        {
            var loc = _context.Locations.Where(l => l.Name.Contains(Name) && l.DeleteDt == null).ToList();
            return loc;
        }
    }
}
