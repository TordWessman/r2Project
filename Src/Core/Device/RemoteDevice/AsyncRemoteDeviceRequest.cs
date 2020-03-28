// This file is part of r2Poject.
//
// Copyright 2016 Tord Wessman
//
// r2Project is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// r2Project is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with r2Project. If not, see <http://www.gnu.org/licenses/>.
//
//
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace R2Core.Device {

    public class AsyncRemoteDeviceTask : Task {

        public AsyncRemoteDeviceTask(Action action) : base(action) { }

    }

    public class AsyncRemoteDeviceRequest {

        private RemoteDevice m_device;
        private Action<dynamic, Exception> m_callback;
        private GetMemberBinder m_getBinder;
        private InvokeMemberBinder m_invokeBinder;
        private object[] m_invokeArgs;
        private SetMemberBinder m_setBinder;
        private object m_setArg;
        private string m_propertyName;
        private bool m_didCallBack;

        public AsyncRemoteDeviceRequest(string propertyName, Action<dynamic, Exception> callback, RemoteDevice remoteDevice) {

            m_propertyName = propertyName;
            m_callback = callback;
            m_device = remoteDevice;

        }

        public AsyncRemoteDeviceRequest(Action<dynamic, Exception> callback, RemoteDevice remoteDevice, SetMemberBinder binder, object arg) {

            m_device = remoteDevice;
            m_callback = callback;
            m_setBinder = binder;
            m_setArg = arg;

        }

        public AsyncRemoteDeviceRequest(Action<dynamic, Exception> callback, RemoteDevice remoteDevice, GetMemberBinder binder) {

            m_device = remoteDevice;
            m_callback = callback;
            m_getBinder = binder;

        }

        public AsyncRemoteDeviceRequest(Action<dynamic, Exception> callback, RemoteDevice remoteDevice, InvokeMemberBinder binder, object[] args) {

            m_device = remoteDevice;
            m_callback = callback;
            m_invokeBinder = binder;
            m_invokeArgs = args;

        }

        public Task Get() {

            AsyncRemoteDeviceTask task = new AsyncRemoteDeviceTask(() => {

                object asyncResult = default(dynamic);

                try {

                    if (m_getBinder != null) {

                        m_device.TryGetMember(m_getBinder, out asyncResult);

                    } else if (m_propertyName != null) {

                        asyncResult = m_device.Get(m_propertyName);

                    } else {

                        throw new DeviceException("Unable to fetch remote value: no propertyName or binder set.");

                    }

                    m_didCallBack = true;
                    m_callback(asyncResult, null);

                } catch (Exception ex) {

                    if (!m_didCallBack) { m_callback(asyncResult, ex); } else { Log.x(ex); }

                }

            });

            m_device.AddTask(task);
            return task;

        }

        public Task Invoke() {

            AsyncRemoteDeviceTask task = new AsyncRemoteDeviceTask(() => {

                object asyncResult = default(dynamic);

                try {

                    m_device.TryInvokeMember(m_invokeBinder, m_invokeArgs, out asyncResult);
                    m_didCallBack = true;
                    m_callback(asyncResult, null);

                } catch (Exception ex) {

                    if (!m_didCallBack) { m_callback(asyncResult, ex); } else { Log.x(ex); }

                }

            });

            m_device.AddTask(task);
            return task;

        }

        public Task Set() {

            AsyncRemoteDeviceTask task = new AsyncRemoteDeviceTask(() => {

                try {

                    m_device.TrySetMember(m_setBinder, m_setArg);
                    m_didCallBack = true;
                    m_callback(true, null);

                } catch (Exception ex) {

                    if (!m_didCallBack) { m_callback(false, ex); } else { Log.x(ex); }

                }

            });

            m_device.AddTask(task);
            return task;

        }

    }

}
