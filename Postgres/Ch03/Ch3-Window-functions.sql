-- Drop and recreate schema for isolation
DROP SCHEMA IF EXISTS streaming CASCADE;
CREATE SCHEMA streaming;

CREATE TABLE streaming.plays (
    play_id       SERIAL PRIMARY KEY,
    song_id       INT NOT NULL,
    song_title    VARCHAR(100) NOT NULL,
    artist        VARCHAR(100) NOT NULL,
    genre         VARCHAR(50) NOT NULL,
    user_id       INT NOT NULL,
    country       VARCHAR(50) NOT NULL,
    play_duration INT NOT NULL,           -- seconds actually played
    song_length   INT NOT NULL,           -- total length of the song
    played_at     TIMESTAMP NOT NULL,
    device        VARCHAR(20) NOT NULL    -- 'mobile', 'desktop', 'smart_speaker'
);

INSERT INTO streaming.plays
    (song_id, song_title, artist, genre, user_id, country, play_duration, song_length, played_at, device)
VALUES
    -- Song 101: "Midnight Drive" by Neon Pulse (electronic, 240s)
    (101, 'Midnight Drive', 'Neon Pulse', 'Electronic', 1, 'USA',    240, 240, '2026-05-01 08:15:00', 'mobile'),
    (101, 'Midnight Drive', 'Neon Pulse', 'Electronic', 2, 'UK',     180, 240, '2026-05-01 09:22:00', 'desktop'),
    (101, 'Midnight Drive', 'Neon Pulse', 'Electronic', 3, 'Germany', 45, 240, '2026-05-01 10:05:00', 'mobile'),
    (101, 'Midnight Drive', 'Neon Pulse', 'Electronic', 4, 'USA',    240, 240, '2026-05-01 14:30:00', 'smart_speaker'),
    (101, 'Midnight Drive', 'Neon Pulse', 'Electronic', 5, 'France', 220, 240, '2026-05-02 07:11:00', 'mobile'),
    (101, 'Midnight Drive', 'Neon Pulse', 'Electronic', 1, 'USA',    240, 240, '2026-05-03 19:45:00', 'desktop'),

    -- Song 102: "Acoustic Sunrise" by Mira Holt (acoustic, 195s)
    (102, 'Acoustic Sunrise', 'Mira Holt', 'Acoustic', 2, 'UK',     195, 195, '2026-05-01 06:30:00', 'smart_speaker'),
    (102, 'Acoustic Sunrise', 'Mira Holt', 'Acoustic', 6, 'Canada', 195, 195, '2026-05-01 07:00:00', 'mobile'),
    (102, 'Acoustic Sunrise', 'Mira Holt', 'Acoustic', 7, 'USA',     90, 195, '2026-05-01 12:15:00', 'desktop'),
    (102, 'Acoustic Sunrise', 'Mira Holt', 'Acoustic', 3, 'Germany',195, 195, '2026-05-02 08:20:00', 'mobile'),
    (102, 'Acoustic Sunrise', 'Mira Holt', 'Acoustic', 8, 'Spain',  150, 195, '2026-05-02 18:00:00', 'mobile'),

    -- Song 103: "Bass Drop City" by DJ Volt (electronic, 210s)
    (103, 'Bass Drop City', 'DJ Volt', 'Electronic', 1, 'USA',     30, 210, '2026-05-01 22:00:00', 'mobile'),
    (103, 'Bass Drop City', 'DJ Volt', 'Electronic', 4, 'USA',    210, 210, '2026-05-01 23:15:00', 'mobile'),
    (103, 'Bass Drop City', 'DJ Volt', 'Electronic', 5, 'France', 210, 210, '2026-05-02 21:00:00', 'desktop'),
    (103, 'Bass Drop City', 'DJ Volt', 'Electronic', 9, 'Brazil', 175, 210, '2026-05-03 20:30:00', 'mobile'),

    -- Song 104: "Coffee Shop Jazz" by The Velvet Trio (jazz, 320s)
    (104, 'Coffee Shop Jazz', 'The Velvet Trio', 'Jazz', 6, 'Canada', 320, 320, '2026-05-01 09:00:00', 'smart_speaker'),
    (104, 'Coffee Shop Jazz', 'The Velvet Trio', 'Jazz', 7, 'USA',    320, 320, '2026-05-02 09:30:00', 'desktop'),
    (104, 'Coffee Shop Jazz', 'The Velvet Trio', 'Jazz', 2, 'UK',     280, 320, '2026-05-03 10:00:00', 'smart_speaker'),
    (104, 'Coffee Shop Jazz', 'The Velvet Trio', 'Jazz', 10,'Japan',  320, 320, '2026-05-03 14:00:00', 'mobile'),

    -- Song 105: "Rage Against Mondays" by Steel Howl (rock, 260s)
    (105, 'Rage Against Mondays', 'Steel Howl', 'Rock', 9, 'Brazil', 260, 260, '2026-05-01 17:00:00', 'mobile'),
    (105, 'Rage Against Mondays', 'Steel Howl', 'Rock', 4, 'USA',    260, 260, '2026-05-02 17:30:00', 'desktop'),
    (105, 'Rage Against Mondays', 'Steel Howl', 'Rock', 8, 'Spain',   60, 260, '2026-05-03 11:00:00', 'mobile');
	
	
 
SET search_path to streaming;

--1

select song_id, played_at, play_duration,
sum (play_duration)
 over (partition by song_id order by played_at)
from plays
order by played_at

--2


with totals as (
select genre, user_id, sum(play_duration)  as total_listening_seconds
from plays
group by genre, user_id
)

select genre, user_id, total_listening_seconds,
DENSE_RANK() OVER (PARTITION BY genre ORDER BY total_listening_seconds DESC) as rank
from totals
 ORDER BY genre, rank;
 
 --3
 
 
with p as (
select *, play_duration * 100.0 / song_length as completion_pct from plays
)

select play_id, song_id, user_id, completion_pct,
Round(avg(completion_pct) over (
  partition by song_id
),2) as avg_completion_pct_for_song

from p

--4

SET search_path to streaming;

select user_id, played_at, song_id ,
lag (song_id) over (PARTITION by user_id order by played_at) as previous_song_id,
played_at - lag (played_at) over (PARTITION by user_id order by   played_at) as minutes_since_previous_play

from plays
order by user_id, played_at

--5

WITH r as (
select song_id, country, sum(play_duration) as tdur from plays
group by song_id, country
),
q as (
 select *,
 dense_rank() OVER (PARTITION BY Country ORDER BY tdur DESC) as srank
 from r
)
select * from q 
where srank<=2

 