﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace conv_gui
{
	class ConvertProcessor
	{
		private static bool						m_process	= false;
		private static bool						m_cancel	= false;
		private static MainForm					m_form		= null;
		private static List<ListViewItem>		m_elements	= null;

		private static List<Thread>				m_threads	= new List<Thread>();
		private static List<List<ListViewItem>>	m_routines	= new List<List<ListViewItem>>();
		
		private static void thread_routine( object reference ){
			List<ListViewItem> pool = reference as List<ListViewItem>;
			bool do_cancel = false;

			lock( m_form ){
				do_cancel = m_cancel;
			};

			while( !do_cancel && ( 0 < pool.Count ) ){								
				bool mod_flag = true;
				bool do_mod = false;

				ListViewItem li = pool.First();
				conv_core.cImageFile src_img = li.Tag as conv_core.cImageFile;

				if( src_img.enabled ){
					int coverted = 0;
					foreach( ColumnHeader hdr in m_form.m_formats ){
						if( !(li.SubItems[ hdr.Index ].Tag as conv_core.cImageFile).enabled ){
							li.SubItems[ hdr.Index ].BackColor = MainForm.cell_back_disabled;
							coverted++;
							continue;
						};
					
						conv_core.cFormat fmt = hdr.Tag as conv_core.cFormat;
						ListViewItem.ListViewSubItem lsi = li.SubItems[ hdr.Index ];
						if( fmt.convert( src_img, lsi.Tag as conv_core.cImageFile ) ){
							lsi.BackColor = Color.LightGreen;
							coverted++;
						}else{
							lsi.BackColor = Color.LightPink;
						};
					};

					if( m_form.m_formats.Count == coverted ){
						li.BackColor = MainForm.cell_back_done;
						
						if( mod_flag && ( src_img.crc != src_img.new_crc ) ){
							do_mod = true;
						};

						src_img.crc = src_img.new_crc;
					}else if( src_img.crc == src_img.new_crc ){
						li.BackColor = MainForm.cell_back_error;
					};

					src_img.close();
				};

				pool.RemoveAt( 0 );

				lock( m_form ){
					m_form.Invoke( new Action( () => {
						m_form.pb_progress.PerformStep();
					} ) );

					if( mod_flag && do_mod ){
						m_form.Invoke( new Action( () => {
							m_form.t_mod.Enabled = true;
						} ) );
						mod_flag = false;
					};

					do_cancel = m_cancel;
				};
			};
		}

		private static void thread_factory(){
			m_process = true;
			int items_count = 0;
			int threads_count = Environment.ProcessorCount / 2;
			m_form.Invoke( new Action( () => {
				m_form.b_process.Enabled			= false;
				m_form.b_progress_cancel.Enabled	= true;
				items_count = ( null == m_elements )? m_form.lv_files.Items.Count : m_elements.Count;
			} ) );
			
			if( ( 2 > ( items_count / Environment.ProcessorCount ) ) || ( 1 > threads_count ) ){
				threads_count = 1;
			};

			for( int proc_id = 0; threads_count > proc_id; proc_id++ ){
				m_threads.Add( new Thread( thread_routine ) );
				m_routines.Add( new List<ListViewItem>() );
			};

			int pool = 0;

			items_count = 0;
			m_form.Invoke( new Action( () => {
				m_form.lv_files.BeginUpdate();
				
				if( null == m_elements ){
					foreach( ListViewItem li in m_form.lv_files.Items ){
						conv_core.cImageFile img = li.Tag as conv_core.cImageFile;
						if( img.enabled ){
							img.new_crc		= conv_core.workbench.file_crc( img.path );
							li.BackColor	= ( img.crc == img.new_crc )? MainForm.cell_back_normal : MainForm.cell_back_modified;
							foreach( ListViewItem.ListViewSubItem lsi in li.SubItems ){
								lsi.BackColor = MainForm.cell_back_normal;
							};
						
							m_routines[ pool++ ].Add( li );
							if( m_routines.Count <= pool ){
								pool = 0;
							};

							items_count++;
						};
					};
				}else{
					foreach( ListViewItem li in m_elements ){
						conv_core.cImageFile img = li.Tag as conv_core.cImageFile;
						if( img.enabled ){
							li.BackColor = ( img.crc == img.new_crc )? MainForm.cell_back_normal : MainForm.cell_back_modified;
							foreach( ListViewItem.ListViewSubItem lsi in li.SubItems ){
								lsi.BackColor = MainForm.cell_back_normal;
							};
						
							m_routines[ pool++ ].Add( li );
							if( m_routines.Count <= pool ){
								pool = 0;
							};

							items_count++;
						};
					};
				};
				
				m_form.lv_files.EndUpdate();
			} ) );

			m_cancel = false;
			for( int id = 0; m_threads.Count > id; id++ ){
				m_threads[ id ].Start( m_routines[ id ] );
			};

			Thread.Sleep( 0 );

			m_form.Invoke( new Action( () => {
				m_form.pb_progress.Visible	= true;
				m_form.pb_progress.Value	= 0;
				m_form.pb_progress.Maximum	= items_count;
			} ) );

			while( !m_cancel ){
				int score = 0;
				
				foreach( List<ListViewItem> fd in m_routines ){
					score += fd.Count;
				};

				if( 0 == score ){
					break;
				};

				Thread.Sleep( 10 );
			};

			m_cancel = true;
			m_form.Invoke( new Action( () => {
				m_form.pb_progress.Visible = false;
			} ) );

			for( int id = 0; m_threads.Count > id; id++ ){
				m_threads[ id ].Join();
				m_routines[ id ].Clear();
			};

			m_routines.Clear();
			m_threads.Clear();

			m_form.Invoke( new Action( () => {
				m_form.b_process.Enabled			= true;
				m_form.b_progress_cancel.Enabled	= false;
			} ) );

			m_process	= false;
			m_form		= null;
		}

		public static void start( MainForm form ){
			m_form		= form;
			m_elements	= null;
			Thread fd	= new Thread( new ThreadStart( thread_factory ) );
			fd.Start();
		}

		public static void start( MainForm form, List<ListViewItem> elements ){
			m_form		= form;
			m_elements	= elements;
			Thread fd	= new Thread( new ThreadStart( thread_factory ) );
			fd.Start();
		}

		public static void cancel(){
			lock( m_form ){
				m_cancel = true;
			};
		}

		public static bool in_process {
			get { return m_process && !m_cancel; }
		}
	}
}
