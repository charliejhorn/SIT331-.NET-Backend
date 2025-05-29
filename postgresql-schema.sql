--
-- PostgreSQL database dump
--

-- Dumped from database version 14.17 (Homebrew)
-- Dumped by pg_dump version 14.17 (Homebrew)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: map; Type: TABLE; Schema: public; Owner: cjhorn
--

CREATE TABLE public.map (
    id integer NOT NULL,
    columns integer NOT NULL,
    rows integer NOT NULL,
    issquare boolean GENERATED ALWAYS AS (((rows > 0) AND (rows = columns))) STORED,
    name character varying(50) NOT NULL,
    description character varying(800),
    createddate timestamp without time zone NOT NULL,
    modifieddate timestamp without time zone NOT NULL
);


ALTER TABLE public.map OWNER TO cjhorn;

--
-- Name: map_id_seq; Type: SEQUENCE; Schema: public; Owner: cjhorn
--

ALTER TABLE public.map ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.map_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: robotcommand; Type: TABLE; Schema: public; Owner: cjhorn
--

CREATE TABLE public.robotcommand (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    description character varying(800),
    ismovecommand boolean NOT NULL,
    createddate timestamp without time zone NOT NULL,
    modifieddate timestamp without time zone NOT NULL
);


ALTER TABLE public.robotcommand OWNER TO cjhorn;

--
-- Name: robotcommand_id_seq; Type: SEQUENCE; Schema: public; Owner: cjhorn
--

ALTER TABLE public.robotcommand ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.robotcommand_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: usermodel; Type: TABLE; Schema: public; Owner: cjhorn
--

CREATE TABLE public.usermodel (
    id integer NOT NULL,
    email character varying(50) NOT NULL,
    firstname character varying(50) NOT NULL,
    lastname character varying(50) NOT NULL,
    passwordhash character varying(100) NOT NULL,
    description character varying(800),
    role character varying(50),
    createddate timestamp without time zone NOT NULL,
    modifieddate timestamp without time zone NOT NULL
);


ALTER TABLE public.usermodel OWNER TO cjhorn;

--
-- Name: usermodel_id_seq; Type: SEQUENCE; Schema: public; Owner: cjhorn
--

ALTER TABLE public.usermodel ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.usermodel_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- PostgreSQL database dump complete
--

