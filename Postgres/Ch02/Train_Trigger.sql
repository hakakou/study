--ROLLBACK;
DROP SCHEMA library cascade;
CREATE SCHEMA IF NOT EXISTS library;

-- Books table with a denormalized copy count
CREATE TABLE library.books (
    id SERIAL PRIMARY KEY,
    title TEXT NOT NULL,
    author TEXT NOT NULL,
    total_copies INTEGER NOT NULL DEFAULT 0,
    available_copies INTEGER NOT NULL DEFAULT 0
);

-- Individual physical copies of each book
CREATE TABLE library.book_copies (
    id SERIAL PRIMARY KEY,
    book_id INTEGER NOT NULL REFERENCES library.books(id) ON DELETE CASCADE,
    barcode TEXT UNIQUE NOT NULL,
    status TEXT NOT NULL CHECK (status IN ('available', 'borrowed', 'lost', 'maintenance'))
);

-- Seed books
INSERT INTO library.books (title, author) VALUES
    ('The Pragmatic Programmer', 'Andy Hunt'),
    ('Designing Data-Intensive Applications', 'Martin Kleppmann'),
    ('Database Internals', 'Alex Petrov');

-- Seed copies
INSERT INTO library.book_copies (book_id, barcode, status) VALUES
    (1, 'PP-001', 'available'),
    (1, 'PP-002', 'available'),
    (1, 'PP-003', 'borrowed'),
    (2, 'DDIA-001', 'available'),
    (2, 'DDIA-002', 'borrowed'),
    (2, 'DDIA-003', 'maintenance'),
    (3, 'DBI-001', 'available');

-- Note: total_copies and available_copies are intentionally still 0.
-- Your trigger should populate them going forward.


create or replace function library.sync_book_counts() 
returns trigger as $$
begin

  update library.books set total_copies = (
    select count(*) from library.book_copies bc
	where bc.book_id = COALESCE(NEW.book_id, OLD.book_id) 
  )
  ,
  available_copies = (
    select count(*) from library.book_copies bc
	where bc.book_id = COALESCE(NEW.book_id, OLD.book_id)
	  and status='available'
  )
  where id = COALESCE(NEW.book_id, OLD.book_id) ;

  IF TG_OP = 'UPDATE' AND NEW.book_id IS DISTINCT FROM OLD.book_id THEN
	  update library.books set total_copies = (
	    select count(*) from library.book_copies bc
		where bc.book_id =  OLD.book_id
	  )
	  ,
	  available_copies = (
	    select count(*) from library.book_copies bc
		where bc.book_id = OLD.book_id
		  and status='available'
	  )
	  where id =  OLD.book_id ;
  END IF;
  RETURN COALESCE(NEW, OLD);
end;
$$ language plpgsql;

create trigger trigger_book_copies
after insert or update or delete on library.book_copies 
FOR EACH ROW
EXECUTE FUNCTION sync_book_counts();

UPDATE library.book_copies SET book_id=book_id;

-- Tests
-- INSERT INTO library.book_copies (book_id, barcode, status) VALUES (1, 'PP-004', 'available');
-- UPDATE library.book_copies SET status = 'borrowed' WHERE barcode = 'DDIA-001';
-- DELETE FROM library.book_copies WHERE barcode = 'DDIA-001';
UPDATE library.book_copies SET book_id = 2 WHERE barcode = 'PP-002';

SELECT * FROM  library.books order by id
