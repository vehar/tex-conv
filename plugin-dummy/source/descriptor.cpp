#include "descriptor.h"
#include <stdio.h>

namespace dll {	
	const char g_dummy_name[] = "Test dummy converter";
	const char g_dummy_desc[] = "Just a test dummy converter";

	plugin::logger_t* g_logger = NULL;
	
	dummy_desc_t::dummy_desc_t(){
	
	};

	dummy_desc_t::~dummy_desc_t(){
		g_logger = NULL;
	};

	const char* dummy_desc_t::name() const {
		return g_dummy_name;
	};

	const char* dummy_desc_t::desc() const {
		return g_dummy_desc;
	};
	
	const int dummy_desc_t::operators_count() const {
		return 1;
	};

	plugin::operator_t* dummy_desc_t::get_operator( const int index ){
		return ( index )? NULL : &m_operator;
	};

	void dummy_desc_t::set_logger( plugin::logger_t* logger_inst ){
		g_logger = logger_inst;
	};

	void send_log( const plugin::log_msg_t type, const char* tag, const char* message ){
		if( g_logger ){
			g_logger->log( type, tag, message );
		};
	};

	void log( const plugin::log_msg_t type, const char* tag, const char* format, va_list vars ){
		static char msg_buffer[ 1024 ];

		if( g_logger ){
			memset( msg_buffer, 0, 1024 );
			vsprintf_s( msg_buffer, format, vars );
			send_log( type, tag, msg_buffer );
		};
	};

	void log( const plugin::log_msg_t type, const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( type, tag, format, args );
		va_end( args );
	};

	void log_v( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_verbose, tag, format, args );
		va_end( args );
	};

	void log_d( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_debug, tag, format, args );
		va_end( args );
	};

	void log_i( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_info, tag, format, args );
		va_end( args );
	};

	void log_n( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_notice, tag, format, args );
		va_end( args );
	};

	void log_w( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_warn, tag, format, args );
		va_end( args );
	};

	void log_er( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_error, tag, format, args );
		va_end( args );
	};

	void log_c( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_crit, tag, format, args );
		va_end( args );
	};

	void log_a( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_alert, tag, format, args );
		va_end( args );
	};

	void log_em( const char* tag, const char* format, ... ){
		va_list args;
		va_start( args, format );
		log( plugin::log_emerg, tag, format, args );
		va_end( args );
	};
};