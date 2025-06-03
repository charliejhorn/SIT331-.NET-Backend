--
-- PostgreSQL database dump
--

-- Dumped from database version 14.18 (Homebrew)
-- Dumped by pg_dump version 14.18 (Homebrew)

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
    is_square boolean GENERATED ALWAYS AS (((rows > 0) AND (rows = columns))) STORED,
    name character varying(50) NOT NULL,
    description character varying(800),
    created_date timestamp without time zone NOT NULL,
    modified_date timestamp without time zone NOT NULL
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
-- Name: robot_command; Type: TABLE; Schema: public; Owner: cjhorn
--

CREATE TABLE public.robot_command (
    id integer NOT NULL,
    name character varying(50) NOT NULL,
    description character varying(800),
    is_move_command boolean NOT NULL,
    created_date timestamp without time zone NOT NULL,
    modified_date timestamp without time zone NOT NULL
);


ALTER TABLE public.robot_command OWNER TO cjhorn;

--
-- Name: robot_command_id_seq; Type: SEQUENCE; Schema: public; Owner: cjhorn
--

ALTER TABLE public.robot_command ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.robot_command_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: user_account; Type: TABLE; Schema: public; Owner: cjhorn
--

CREATE TABLE public.user_account (
    id integer NOT NULL,
    password_hash character varying(200) NOT NULL,
    email character varying(100) NOT NULL,
    description character varying(800),
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    role character varying(20) NOT NULL,
    created_date timestamp without time zone NOT NULL,
    modified_date timestamp without time zone NOT NULL
);


ALTER TABLE public.user_account OWNER TO cjhorn;

--
-- Name: user_account_id_seq; Type: SEQUENCE; Schema: public; Owner: cjhorn
--

ALTER TABLE public.user_account ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.user_account_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: map pk_map; Type: CONSTRAINT; Schema: public; Owner: cjhorn
--

ALTER TABLE ONLY public.map
    ADD CONSTRAINT pk_map PRIMARY KEY (id);


--
-- Name: robot_command pk_robot_command; Type: CONSTRAINT; Schema: public; Owner: cjhorn
--

ALTER TABLE ONLY public.robot_command
    ADD CONSTRAINT pk_robot_command PRIMARY KEY (id);


--
-- Name: user_account pk_user; Type: CONSTRAINT; Schema: public; Owner: cjhorn
--

ALTER TABLE ONLY public.user_account
    ADD CONSTRAINT pk_user PRIMARY KEY (id);


--
-- PostgreSQL database dump complete
--

